# Synthetic training data + sklearn inference for wait-time estimates.
from __future__ import annotations

import random

import numpy as np
from fastapi import FastAPI
from pydantic import BaseModel, Field
from sklearn.linear_model import Ridge

app = FastAPI(title="Queue ML Service", version="1.0.0")


class PredictRequest(BaseModel):
    queue_length: int = Field(ge=0)
    active_counters: int = Field(ge=0)
    service_type: str
    avg_service_minutes: float = Field(ge=0)


class PredictResponse(BaseModel):
    estimated_avg_wait_minutes: float
    estimated_clearing_minutes: float


def _heuristic(req: PredictRequest) -> PredictResponse:
    counters = max(1, req.active_counters)
    avg = req.avg_service_minutes if req.avg_service_minutes > 0 else 5.0
    clearing = req.queue_length / counters * avg
    avg_wait = clearing / 2.0
    return PredictResponse(
        estimated_avg_wait_minutes=round(avg_wait, 1),
        estimated_clearing_minutes=round(clearing, 1),
    )


def _build_training() -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    rng = random.Random(42)
    rows: list[list[float]] = []
    y_wait: list[float] = []
    y_clear: list[float] = []
    for _ in range(800):
        q = rng.randint(0, 60)
        c = max(1, rng.randint(1, 5))
        avg_s = rng.uniform(3, 12)
        svc = rng.choice(["General", "Card", "Wealth"])
        svc_bias = {"General": 1.0, "Card": 0.95, "Wealth": 1.05}[svc]
        noise = rng.gauss(0, 0.8)
        clear = max(0.5, (q / c) * avg_s * svc_bias + noise)
        wait = max(0.5, clear * rng.uniform(0.35, 0.65) + rng.gauss(0, 0.4))
        rows.append([q, c, avg_s, 1.0 if svc == "General" else 0.0, 1.0 if svc == "Card" else 0.0])
        y_wait.append(wait)
        y_clear.append(clear)
    return np.asarray(rows), np.asarray(y_wait), np.asarray(y_clear)


@app.on_event("startup")
def train_model() -> None:
    x, _y_w, y_c = _build_training()
    m_clear = Ridge(alpha=1.0)
    m_clear.fit(x, y_c)
    app.state.model_clear = m_clear


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest) -> PredictResponse:
    try:
        svc = req.service_type.strip()
        enc_g = 1.0 if svc == "General" else 0.0
        enc_c = 1.0 if svc == "Card" else 0.0
        row = np.array([[req.queue_length, req.active_counters, req.avg_service_minutes, enc_g, enc_c]])
        if hasattr(app.state, "model_clear"):
            clear = float(app.state.model_clear.predict(row)[0])
            clear = max(0.5, clear)
            wait = clear * 0.52
            return PredictResponse(
                estimated_avg_wait_minutes=round(wait, 1),
                estimated_clearing_minutes=round(clear, 1),
            )
    except Exception:
        pass
    return _heuristic(req)
