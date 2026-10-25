import sys

import resvg_py

# Usage: render_svg.py <in.svg> <out.png>   (pip install resvg_py)
svg_path, out_path = sys.argv[1], sys.argv[2]
with open(out_path, "wb") as out:
    out.write(bytes(resvg_py.svg_to_bytes(svg_path=svg_path)))
