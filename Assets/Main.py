from cvzone.HandTrackingModule import HandDetector
import cv2
import socket

# Camera setup
cap = cv2.VideoCapture(0)
cap.set(3, 1920)
cap.set(4, 1080)

# Hand detector
detector = HandDetector(detectionCon=0.8, maxHands=2)

# Network configuration
PHONE_IP = "192.168.0.109"   # phone IP
LAPTOP_IP = "192.168.0.102"  # laptop IP
PORT = 5052

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

while True:
    success, img = cap.read()
    if not success:
        continue

    img = cv2.flip(img, 1)
    h, w, _ = img.shape

    hands, img = detector.findHands(img)

    L = []
    R = []

    if hands:
        for hand in hands:
            lmList = hand["lmList"]

            arr = []
            for lm in lmList:
                # x, flipped y, z
                arr.extend([lm[0], h - lm[1], lm[2]])

            # Decide left / right by wrist X position
            palm_x = lmList[0][0]

            if palm_x < w / 2:
                L = arr
            else:
                R = arr

    packet = f"[L:{','.join(map(str, L))}|R:{','.join(map(str, R))}]"

    sock.sendto(packet.encode(), (PHONE_IP, PORT))
    sock.sendto(packet.encode(), (LAPTOP_IP, PORT))

    cv2.imshow("Image", img)
    cv2.waitKey(1)