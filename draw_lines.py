from math import ceil
import drawsvg as draw

feetToInches = 12
inchesToCm = 2.54
cmToM = 1/100

# image dimensions
IMG_WIDTH = IMG_HEIGHT = 2000

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

        self.end_x = end_x
        self.end_y = end_y

class LaneCurve(draw.Path):
    def __init__(self, start_x, start_y, end_x, end_y, ctrl_x, ctrl_y):
        super().__init__(stroke="black", fill="none", stroke_width=ceil(TAPE_WIDTH))

        self.M(start_x, start_y).Q(end_x, start_y, end_x, end_y)

        self.end_x = end_x
        self.end_y = end_y


# at the start (bottom of the image aka eastwards-ish I think)
# straightaways should go the full length of the course, so draw the first half of one
straightAwayStartOutside = LaneLine(center(0), center(COURSE_WIDTH/2), center(COURSE_LENGTH/2), center(COURSE_WIDTH/2))

# draw the first turn
firstTurnOutside = LaneCurve(straightAwayStartOutside.end_x, straightAwayStartOutside.end_y, center(COURSE_LENGTH/2 + MAX_LANE_WIDTH), straightAwayStartOutside.end_y-MAX_LANE_WIDTH, IMG_WIDTH, IMG_HEIGHT)
firstTurnOutside2 = LaneCurve(firstTurnOutside.end_x, firstTurnOutside.end_y, straightAwayStartOutside.end_x, straightAwayStartOutside.end_y-MAX_LANE_WIDTH, IMG_WIDTH, IMG_HEIGHT)



markerOutside = LaneLine(firstTurnOutside.end_x+20, straightAwayStartOutside.end_y, firstTurnOutside.end_x+20, firstTurnOutside.end_y-COURSE_LENGTH)

d = draw.Drawing(IMG_WIDTH, IMG_HEIGHT)
d.append(straightAwayStartOutside)
d.append(firstTurnOutside)
d.append(firstTurnOutside2)
d.append(markerOutside)

d.display_image()
d.save_svg("temp.svg")