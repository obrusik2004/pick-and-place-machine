import cv2
import os
import time
import argparse


def capture_frame(output_dir, device_index=1):
    os.makedirs(output_dir, exist_ok=True)

    cap = cv2.VideoCapture(device_index)
    if not cap.isOpened():
        print("ERROR: Could not open camera")
        return None

    ret, frame = cap.read()
    cap.release()

    if not ret:
        print("ERROR: Could not read frame")
        return None

    ts = time.strftime("%Y%m%d-%H%M%S")
    filename = f"frame_{ts}.png"
    path = os.path.join(output_dir, filename)

    cv2.imwrite(path, frame)
    print(path)  # for C#
    return path


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--outdir", default="captures")
    parser.add_argument("--device", type=int, default=1)
    args = parser.parse_args()

    capture_frame(args.outdir, args.device)