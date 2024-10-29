from math import ceil
import drawsvg as draw

feetToInches = 12
inchesToCm = 2.54
cmToM = 1/100

# image dimensions
IMG_WIDTH = IMG_HEIGHT = 1024

metersToPixels = 30

# course dimensions
COURSE_LENGTH = 120 * feetToInches * inchesToCm * cmToM * metersToPixels
COURSE_WIDTH = 100 * feetToInches * inchesToCm * cmToM * metersToPixels
TAPE_WIDTH = 3 * inchesToCm * cmToM * metersToPixels
POTHOLE_DIAMETER = 2 * feetToInches * inchesToCm * cmToM * metersToPixels
MIN_LANE_WIDTH = 10 * feetToInches * inchesToCm * cmToM * metersToPixels
MAX_LANE_WIDTH = 20 * feetToInches * inchesToCm * cmToM * metersToPixels

def center(x):
    return x + IMG_WIDTH//2

class LaneLine(draw.Line):
    def __init__(self, start_x, start_y, end_x, end_y):
        super().__init__(start_x, start_y, end_x, end_y, stroke="black", fill="none", stroke_width=ceil(TAPE_WIDTH))

# at the start (bottom of the image aka eastwards-ish I think)
# straightaways should go the full length of the course, so draw the first one
straightaway1 = LaneLine()



# straightaway1 = draw.Line(300, 100, COURSE_LENGTH/2, IMG_HEIGHT/2 - TAPE_WIDTH, stroke="black", fill="none", stroke_width=ceil(TAPE_WIDTH))

d = draw.Drawing(IMG_WIDTH, IMG_HEIGHT)
d.append(straightaway1)
# d.append(draw.Line(-100, 0, 100, 0, stroke="black", fill="none", stroke_width=ceil(TAPE_WIDTH)))

d.display_image()
d.save_svg("temp.svg")