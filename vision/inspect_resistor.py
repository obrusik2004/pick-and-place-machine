import argparse
import json
from vision_resistor import analyze_image

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--id", required=True, help="component ID, e.g. R14")
    parser.add_argument("--image", required=True, help="path to image file")
    parser.add_argument(
        "--expected-axis",
        type=float,
        default=0.0,
        help="expected resistor axis in degrees (0 or 90)",
    )
    args = parser.parse_args()

    result = analyze_image(args.image, expected_axis_deg=args.expected_axis)
    result["id"] = args.id

    print(json.dumps(result))
