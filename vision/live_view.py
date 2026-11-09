import cv2
import argparse

parser = argparse.ArgumentParser()
parser.add_argument("--device", type=int, default=2)
args = parser.parse_args()

cap = cv2.VideoCapture(args.device)

if not cap.isOpened():
    print("ERROR: Cannot open camera")
    exit(1)

cv2.namedWindow("USB Camera Live View", cv2.WINDOW_NORMAL)

while True:
    ret, frame = cap.read()
    if not ret:
        print("ERROR: Failed to grab frame")
        break

    h, w = frame.shape[:2]
    cx = w // 2
    cy = h // 2

    size = 15        # half-length of crosshair arms
    color = (0, 255, 0)  
    thickness = 1

    # horizontal line
    cv2.line(frame, (cx - size, cy), (cx + size, cy), color, thickness)
    # vertical line
    cv2.line(frame, (cx, cy - size), (cx, cy + size), color, thickness)

    cv2.imshow("USB Camera Live View", frame)

    key = cv2.waitKey(1) & 0xFF
    if key == ord('q') or key == 27:  # q or ESC
        break

cap.release()
cv2.destroyAllWindows()
