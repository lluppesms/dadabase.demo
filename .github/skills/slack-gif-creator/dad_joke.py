"""
Animated GIF: Dad telling a kid a joke
A dad figure on the left speaks a joke, the kid on the right reacts.
Sequence:
  Phase 1 (frames 0-7):   Dad leans in, speech bubble appears with "Why did the..."
  Phase 2 (frames 8-15):  Speech bubble shows full setup "...scarecrow win an award?"
  Phase 3 (frames 16-23): Kid tilts head (confused)
  Phase 4 (frames 24-31): Dad laughs, punch-line bubble "Because he was outstanding in his field!"
  Phase 5 (frames 32-39): Kid groans - face-palm, stars spin around head
"""

import math
import sys
import os

sys.path.insert(0, os.path.dirname(__file__))

from core.gif_builder import GIFBuilder
from core.easing import interpolate
from PIL import Image, ImageDraw

W, H = 480, 480
FPS = 12
NUM_FRAMES = 40

# ── colour palette ────────────────────────────────────────────────────────────
SKY        = (220, 240, 255)
GROUND     = (160, 220, 120)
SKIN       = (255, 213, 170)
HAIR_DAD   = (100,  65,  20)
HAIR_KID   = (220, 170,  50)
SHIRT_DAD  = ( 70, 130, 180)   # steel blue
SHIRT_KID  = (220,  80,  80)   # red
PANTS_DAD  = ( 50,  50,  80)
PANTS_KID  = ( 80, 100, 180)
BUBBLE     = (255, 255, 240)
BUBBLE_OUT = ( 80,  80,  80)
TEXT_COL   = ( 40,  40,  40)
GROAN_COL  = (180,  60,  60)
LAUGH_COL  = (240, 200,  50)
STAR_COL   = (255, 220,   0)


def draw_gradient_sky(draw):
    for y in range(H):
        r = int(SKY[0] + (GROUND[0] - SKY[0]) * y / H)
        g = int(SKY[1] + (GROUND[1] - SKY[1]) * y / H)
        b = int(SKY[2] + (GROUND[2] - SKY[2]) * y / H)
        draw.line([(0, y), (W, y)], fill=(r, g, b))


def draw_ground(draw):
    draw.rectangle([0, H - 80, W, H], fill=GROUND)
    # grass tufts
    for x in range(10, W, 20):
        draw.polygon(
            [(x, H - 80), (x - 5, H - 92), (x + 5, H - 92)],
            fill=(100, 190, 80)
        )


def draw_house(draw, cx, ground_y, width=260, height=160):
    """Draw a simple house centered at `cx` sitting on the ground at `ground_y`.
    width/height are the house body size (roof sits above).
    """
    body_h = height
    body_w = width
    body_left = cx - body_w // 2
    body_right = cx + body_w // 2
    body_top = ground_y - body_h
    body_bottom = ground_y - 6

    # body
    house_body = (body_left, body_top, body_right, body_bottom)
    draw.rectangle(house_body, fill=(240, 220, 200), outline=(80, 60, 40), width=3)

    # roof
    roof_peak_x = cx
    roof_peak_y = body_top - 50
    draw.polygon([(body_left - 8, body_top), (body_right + 8, body_top), (roof_peak_x, roof_peak_y)],
                 fill=(180, 80, 60), outline=(80, 40, 30))

    # door
    door_w = 40
    door_h = 70
    door_left = cx - door_w // 2
    door_top = body_bottom - door_h
    draw.rectangle([door_left, door_top, door_left + door_w, body_bottom], fill=(120, 70, 40), outline=(40, 20, 10), width=2)
    draw.ellipse([door_left + door_w - 12, door_top + door_h//2 - 6, door_left + door_w - 4, door_top + door_h//2 + 6], fill=(220, 200, 120))

    # windows
    win_w = 44
    win_h = 38
    win_y = body_top + 30
    # left window
    lwx = body_left + 36
    draw.rectangle([lwx, win_y, lwx + win_w, win_y + win_h], fill=(200, 235, 255), outline=(60, 90, 120), width=2)
    draw.line([(lwx + win_w//2, win_y), (lwx + win_w//2, win_y + win_h)], fill=(80, 110, 130), width=2)
    draw.line([(lwx, win_y + win_h//2), (lwx + win_w, win_y + win_h//2)], fill=(80, 110, 130), width=2)
    # right window
    rwx = body_right - 36 - win_w
    draw.rectangle([rwx, win_y, rwx + win_w, win_y + win_h], fill=(200, 235, 255), outline=(60, 90, 120), width=2)
    draw.line([(rwx + win_w//2, win_y), (rwx + win_w//2, win_y + win_h)], fill=(80, 110, 130), width=2)
    draw.line([(rwx, win_y + win_h//2), (rwx + win_w, win_y + win_h//2)], fill=(80, 110, 130), width=2)

    # chimney
    chim_w = 18
    chim_h = 40
    chim_x = body_right - 60
    chim_y = roof_peak_y + 6
    draw.rectangle([chim_x, chim_y, chim_x + chim_w, chim_y + chim_h], fill=(120, 60, 50), outline=(60, 30, 25))
    # smoke puffs
    draw.ellipse([chim_x + 6, chim_y - 18, chim_x + 22, chim_y - 2], fill=(230, 230, 230))
    draw.ellipse([chim_x - 2, chim_y - 32, chim_x + 18, chim_y - 14], fill=(240, 240, 240))


def draw_stick_figure(draw, cx, ground_y, shirt_col, pants_col, hair_col,
                       head_offset_x=0, head_offset_y=0, arm_angle=0,
                       body_lean=0, eye_x_shift=0):
    """Draw a simple but polished stick figure."""
    # proportions
    head_r  = 32
    neck_len = 14
    torso_h  = 80
    leg_h    = 70
    arm_h    = 60

    # key y positions
    head_cy  = ground_y - leg_h - torso_h - neck_len - head_r + head_offset_y
    neck_top = head_cy + head_r
    neck_bot = neck_top + neck_len
    torso_bot = neck_bot + torso_h
    # lean adjustments
    lean = body_lean  # pixels of horizontal lean at torso_bot relative to torso_top

    torso_top_x = cx + head_offset_x
    torso_bot_x = cx + lean

    # head (with slight offset for leaning in)
    hx = torso_top_x
    hy = head_cy
    # shadow
    draw.ellipse([hx - head_r + 4, hy - head_r + 4,
                  hx + head_r + 4, hy + head_r + 4],
                 fill=(180, 150, 120))
    draw.ellipse([hx - head_r, hy - head_r, hx + head_r, hy + head_r],
                 fill=SKIN, outline=(180, 130, 90), width=2)

    # hair
    draw.arc([hx - head_r, hy - head_r, hx + head_r, hy],
             start=180, end=0, fill=hair_col, width=10)

    # eyes
    ex = hx + eye_x_shift
    draw.ellipse([ex - 10, hy - 6, ex - 4, hy + 2], fill=(50, 50, 50))
    draw.ellipse([ex + 4,  hy - 6, ex + 10, hy + 2], fill=(50, 50, 50))

    # smile
    draw.arc([hx - 10, hy + 6, hx + 10, hy + 18], start=10, end=170,
             fill=(160, 60, 60), width=3)

    # neck
    draw.line([(torso_top_x, neck_top), (torso_top_x, neck_bot)],
              fill=SKIN, width=6)

    # torso
    draw.polygon(
        [(torso_top_x - 18, neck_bot),
         (torso_top_x + 18, neck_bot),
         (torso_bot_x + 22, torso_bot),
         (torso_bot_x - 22, torso_bot)],
        fill=shirt_col, outline=(0, 0, 0), width=2
    )

    # arms  (arm_angle: 0 = down, positive = raised)
    a_rad = math.radians(arm_angle)
    # left arm
    lax = torso_top_x - 18 + int(arm_h * math.sin(-a_rad))
    lay = neck_bot + torso_h // 2 + int(arm_h * math.cos(a_rad))
    draw.line([(torso_top_x - 18, neck_bot + torso_h // 2), (lax, lay)],
              fill=SKIN, width=7)
    # right arm
    rax = torso_top_x + 18 + int(arm_h * math.sin(a_rad))
    ray = neck_bot + torso_h // 2 + int(arm_h * math.cos(a_rad))
    draw.line([(torso_top_x + 18, neck_bot + torso_h // 2), (rax, ray)],
              fill=SKIN, width=7)

    # legs
    draw.polygon(
        [(torso_bot_x - 22, torso_bot),
         (torso_bot_x - 4,  torso_bot),
         (torso_bot_x - 4,  torso_bot + leg_h),
         (torso_bot_x - 22, torso_bot + leg_h)],
        fill=pants_col, outline=(0, 0, 0), width=2
    )
    draw.polygon(
        [(torso_bot_x + 4,  torso_bot),
         (torso_bot_x + 22, torso_bot),
         (torso_bot_x + 22, torso_bot + leg_h),
         (torso_bot_x + 4,  torso_bot + leg_h)],
        fill=pants_col, outline=(0, 0, 0), width=2
    )
    # shoes
    draw.ellipse([torso_bot_x - 28, torso_bot + leg_h - 6,
                  torso_bot_x - 2,  torso_bot + leg_h + 12],
                 fill=(60, 40, 20))
    draw.ellipse([torso_bot_x + 2,  torso_bot + leg_h - 6,
                  torso_bot_x + 28, torso_bot + leg_h + 12],
                 fill=(60, 40, 20))


def draw_speech_bubble(draw, tip_x, tip_y, text_lines, align='left',
                        alpha_scale=1.0, color=BUBBLE, text_color=TEXT_COL):
    """Draw a rounded speech bubble with a triangular tail."""
    if alpha_scale <= 0:
        return
    padding = 14
    line_h  = 22
    font_approx_w = 10  # pixels per char
    max_w = max(len(l) * font_approx_w for l in text_lines) + padding * 2
    bubble_h = len(text_lines) * line_h + padding * 2

    if align == 'left':
        bx = tip_x + 20
        by = tip_y - bubble_h - 20
    else:
        bx = tip_x - max_w - 20
        by = tip_y - bubble_h - 20

    # clamp to canvas
    bx = max(8, min(W - max_w - 8, bx))
    by = max(8, by)

    r = 14  # corner radius
    # bubble body
    draw.rounded_rectangle([bx, by, bx + max_w, by + bubble_h],
                             radius=r, fill=color, outline=BUBBLE_OUT, width=3)
    # tail triangle pointing to tip_x, tip_y
    tail_base_x = bx + 30 if align == 'left' else bx + max_w - 30
    draw.polygon(
        [(tail_base_x - 10, by + bubble_h),
         (tail_base_x + 10, by + bubble_h),
         (tip_x, tip_y)],
        fill=color, outline=BUBBLE_OUT
    )

    # text (use default PIL font - small but readable)
    for i, line in enumerate(text_lines):
        tx = bx + padding
        ty = by + padding + i * line_h
        draw.text((tx, ty), line, fill=text_color)


def draw_groan_stars(draw, cx, cy, frame, num_stars=6):
    """Spinning groan stars around the kid's head."""
    for s in range(num_stars):
        angle = math.radians(frame * 18 + s * (360 / num_stars))
        r = 44
        sx = cx + int(r * math.cos(angle))
        sy = cy + int(r * math.sin(angle))
        size = 10
        pts = []
        for p in range(10):
            a = math.radians(p * 36 - 90)
            rd = size if p % 2 == 0 else size * 0.4
            pts.append((sx + rd * math.cos(a), sy + rd * math.sin(a)))
        draw.polygon(pts, fill=GROAN_COL)


def draw_laugh_lines(draw, cx, cy, frame):
    """Wavy laugh lines radiating from the dad's face."""
    for i in range(6):
        angle = math.radians(i * 60 + frame * 15)
        for d in [35, 50, 65]:
            x = cx + int(d * math.cos(angle))
            y = cy + int(d * math.sin(angle))
            draw.ellipse([x-4, y-4, x+4, y+4], fill=LAUGH_COL)


builder = GIFBuilder(width=W, height=H, fps=FPS)

GROUND_Y = H - 80   # y where feet touch ground

DAD_X  = 140        # dad center x
KID_X  = 340        # kid center x

# head top of dad (approx) for bubble tip
DAD_HEAD_Y = GROUND_Y - 70 - 80 - 14 - 32   # ground - leg - torso - neck - head_r
KID_HEAD_Y = DAD_HEAD_Y + 20                  # kid is a bit shorter / same

PHASES = {
    'lean_in':    (0,  7),
    'setup':      (8,  15),
    'confused':   (16, 23),
    'punchline':  (24, 31),
    'groan':      (32, 39),
}


def phase_t(frame, start, end):
    span = end - start
    return max(0.0, min(1.0, (frame - start) / span))


for f in range(NUM_FRAMES):
    img  = Image.new('RGB', (W, H), SKY)
    draw = ImageDraw.Draw(img)

    draw_gradient_sky(draw)
    draw_ground(draw)
    # draw house centered between characters
    draw_house(draw, (DAD_X + KID_X) // 2, GROUND_Y, width=260, height=140)

    # ── phase helpers ────────────────────────────────────────────────────────
    t_lean      = phase_t(f, *PHASES['lean_in'])
    t_setup     = phase_t(f, *PHASES['setup'])
    t_confused  = phase_t(f, *PHASES['confused'])
    t_punch     = phase_t(f, *PHASES['punchline'])
    t_groan     = phase_t(f, *PHASES['groan'])

    # ── Dad position / animation ─────────────────────────────────────────────
    dad_lean = int(interpolate(0, 20, t_lean, 'ease_out'))
    dad_arm  = int(interpolate(0, 40, t_lean, 'ease_out'))

    # laughing: head bobs up/down
    if t_punch > 0:
        laugh_bob = int(8 * math.sin(f * 1.2))
    else:
        laugh_bob = 0

    # dad raises arm further during punchline
    if t_punch > 0:
        dad_arm = 60

    draw_stick_figure(draw, DAD_X, GROUND_Y,
                       SHIRT_DAD, PANTS_DAD, HAIR_DAD,
                       head_offset_x=dad_lean,
                       head_offset_y=laugh_bob,
                       arm_angle=dad_arm,
                       body_lean=dad_lean,
                       eye_x_shift=4)

    # ── Kid position / animation ─────────────────────────────────────────────
    # confused: head tilt = oscillate
    kid_head_x = 0
    if t_confused > 0:
        tilt_amount = int(12 * math.sin(t_confused * math.pi * 2))
        kid_head_x = tilt_amount

    # groan: face-palm - lean forward
    kid_lean = 0
    kid_arm  = 0
    if t_groan > 0:
        kid_lean = int(interpolate(0, -15, t_groan, 'ease_in_out'))
        kid_arm  = int(interpolate(0, -70, t_groan, 'ease_in_out'))  # arm up to face

    draw_stick_figure(draw, KID_X, GROUND_Y,
                       SHIRT_KID, PANTS_KID, HAIR_KID,
                       head_offset_x=kid_head_x,
                       arm_angle=kid_arm,
                       body_lean=kid_lean,
                       eye_x_shift=-4)

    # ── Speech bubbles ────────────────────────────────────────────────────────
    bubble_tip_x = DAD_X + dad_lean + 36
    bubble_tip_y = DAD_HEAD_Y + 10

    if PHASES['lean_in'][0] <= f <= PHASES['lean_in'][1]:
        alpha = t_lean
        if alpha > 0.3:
            draw_speech_bubble(draw, bubble_tip_x, bubble_tip_y,
                                ["Why did the"], align='left')

    elif PHASES['setup'][0] <= f <= PHASES['setup'][1]:
        draw_speech_bubble(draw, bubble_tip_x, bubble_tip_y,
                            ["Why did the", "scarecrow win", "an award?"],
                            align='left')

    elif PHASES['confused'][0] <= f <= PHASES['confused'][1]:
        # kid's question bubble
        kid_bubble_tip_x = KID_X - 36
        kid_bubble_tip_y = KID_HEAD_Y + 10
        draw_speech_bubble(draw, kid_bubble_tip_x, kid_bubble_tip_y,
                            ["Um...why?"], align='right',
                            color=(240, 255, 240))

    elif PHASES['punchline'][0] <= f <= PHASES['punchline'][1]:
        draw_speech_bubble(draw, bubble_tip_x, bubble_tip_y,
                            ["Because he was", "outstanding in", "his field! 😄"],
                            align='left', color=(255, 255, 200))
        draw_laugh_lines(draw,
                          DAD_X + dad_lean,
                          DAD_HEAD_Y + laugh_bob,
                          f)

    elif PHASES['groan'][0] <= f <= PHASES['groan'][1]:
        kid_bubble_tip_x = KID_X - 36
        kid_bubble_tip_y = KID_HEAD_Y + 10
        draw_speech_bubble(draw, kid_bubble_tip_x, kid_bubble_tip_y,
                            ["Daaad...", "*groan*"],
                            align='right', color=(255, 220, 220),
                            text_color=GROAN_COL)
        # keep dad's punchline fading
        draw_speech_bubble(draw, bubble_tip_x, bubble_tip_y,
                            ["Heh heh heh..."],
                            align='left', color=(255, 255, 200))
        draw_groan_stars(draw, KID_X + kid_head_x, KID_HEAD_Y, f)

    builder.add_frame(img)

out_path = os.path.join(os.path.dirname(__file__), 'dad_joke.gif')
builder.save(out_path, num_colors=128, optimize_for_emoji=False)
print(f"Saved → {out_path}")
