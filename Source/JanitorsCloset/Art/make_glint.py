import sys
import numpy as np
from PIL import Image

out = sys.argv[1]
size = 128
c = (size - 1) / 2
y, x = np.mgrid[0:size, 0:size]
dx, dy = (x - c) / c, (y - c) / c
r = np.sqrt(dx * dx + dy * dy)

# Four-point star: two thin crossed rays over a soft core, fading to the edge.
rays = np.exp(-np.abs(dx) * 22) * np.exp(-np.abs(dy) * 2.2) + np.exp(-np.abs(dy) * 22) * np.exp(-np.abs(dx) * 2.2)
core = np.exp(-(r * 5.5) ** 2)
halo = 0.25 * np.exp(-(r * 2.4) ** 2)
alpha = np.clip(rays * 0.9 + core + halo, 0, 1) * np.clip(1 - r, 0, 1)

rgba = np.zeros((size, size, 4), dtype=np.uint8)
rgba[..., :3] = 255
rgba[..., 3] = (alpha * 255).astype(np.uint8)
Image.fromarray(rgba, "RGBA").save(out)
print("wrote", out)
