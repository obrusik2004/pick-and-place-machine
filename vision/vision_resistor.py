import cv2
import numpy as np
import math

ROI_SIZE = 480
SCALE_MM_PER_PX = 0.01661


BODY_MIN_AREA = 6000
BODY_MAX_AREA = 10000
BODY_ASPECT_MIN = 1
BODY_ASPECT_MAX = 2.5
BODY_MIN_SOLIDITY = 0.7

BODY_G_RAW_MAX = 11.0
BODY_G_RAW_MIN = -5

BODY_V_MIN = 22
BODY_V_MAX = 100


def central_roi(img, size):
    h, w = img.shape[:2]
    cx, cy = w // 2, h // 2
    half = size // 2
    x0, y0 = max(0, cx - half), max(0, cy - half)
    x1, y1 = min(w, cx + half), min(h, cy + half)
    return img[y0:y1, x0:x1]


def rect_geometry(rect):
    (cx, cy), (w, h), angle = rect

    # long side is w
    if w < h:
        w, h = h, w
        angle += 90.0

    L, S = w, h
    return (cx, cy), L, S, angle


def angle_error(main_angle_deg, expected_axis_deg):
    a = main_angle_deg % 180
    t = expected_axis_deg % 180
    d = a - t
    if d > 90:
        d -= 180
    if d < -90:
        d += 180
    return d


def compute_offsets(res_center, angle_deg, img_shape, expected_axis_deg=0.0):
    h, w = img_shape[:2]
    cx_pad = w / 2
    cy_pad = h / 2

    cx_res, cy_res = res_center
    dx_img = cx_res - cx_pad
    dy_img = cy_res - cy_pad

    dx_machine_px = dx_img
    dy_machine_px = dy_img

    dx_mm = dx_machine_px * SCALE_MM_PER_PX
    dy_mm = dy_machine_px * SCALE_MM_PER_PX

    dtheta = angle_error(angle_deg, expected_axis_deg)

    dist_mm = math.sqrt(dx_img**2 + dy_img**2) * SCALE_MM_PER_PX

    return dx_mm, dy_mm, dtheta, dist_mm


def detect_resistor_body(img_bgr):
    img_bgr = cv2.bilateralFilter(img_bgr, 4, 8, 10)
    b, g, r = cv2.split(img_bgr)
    gs = g.astype(np.float32) - 0.5 * r.astype(np.float32) - 0.5 * b.astype(np.float32)

    hsv = cv2.cvtColor(img_bgr, cv2.COLOR_BGR2HSV)
    _, _, v = cv2.split(hsv)

    mask = (
        (gs >= BODY_G_RAW_MIN)
        & (gs <= BODY_G_RAW_MAX)
        & (v >= BODY_V_MIN)
        & (v <= BODY_V_MAX)
    ).astype(np.uint8) * 255

    k = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, k, iterations=1)
    mask = cv2.morphologyEx(
        mask, cv2.MORPH_CLOSE, np.ones((3, 3), np.uint8), iterations=1
    )
    mask = cv2.medianBlur(mask, 5)

    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    if not contours:
        return None

    best_rect = None
    best_score = -1

    for c in contours:
        area = cv2.contourArea(c)
        if area < BODY_MIN_AREA or area > BODY_MAX_AREA:
            continue

        rect = cv2.minAreaRect(c)
        (cx, cy), (w, h), angle = rect

        box = cv2.boxPoints(rect).astype(int)
        m = 5
        if (
            box[:, 0].min() <= m
            or box[:, 1].min() <= m
            or box[:, 0].max() >= img_bgr.shape[1] - m
            or box[:, 1].max() >= img_bgr.shape[0] - m
        ):
            continue

        L = max(w, h)
        S = min(w, h)
        aspect = L / S if S > 0 else 999

        hull = cv2.contourArea(cv2.convexHull(c))
        solidity = area / hull if hull > 0 else 0

        if aspect < BODY_ASPECT_MIN or aspect > BODY_ASPECT_MAX:
            continue
        if solidity < BODY_MIN_SOLIDITY:
            continue

        score = area * solidity
        if score > best_score:
            best_score = score
            best_rect = rect

    return best_rect


# Main function


def analyze_image(image_path, expected_axis_deg=0.0):
    img = cv2.imread(image_path)
    if img is None:
        return {"status": "ERROR", "message": "Could not load image"}

    roi = central_roi(img, ROI_SIZE)
    rect = detect_resistor_body(roi)

    if rect is None:
        return {"status": "NOT_FOUND"}

    (cx, cy), L, S, angle = rect_geometry(rect)

    dx_mm, dy_mm, dtheta, dist_mm = compute_offsets(
        (cx, cy), angle, roi.shape, expected_axis_deg
    )

    # tolerances
    needs = abs(dx_mm) > 0.35 or abs(dy_mm) > 0.35 or abs(dtheta) > 10.0

    return {
        "status": "NEEDS_CORRECTION" if needs else "OK",
        "cx_px": round(cx, 2),
        "cy_px": round(cy, 2),
        "angle_deg": round(angle, 2),
        "dx_mm": round(dx_mm, 3),
        "dy_mm": round(dy_mm, 3),
        "dtheta_deg": round(dtheta, 2),
        "dist_mm": round(dist_mm, 3),
    }
