
import cv2
import matplotlib.pyplot as plt
import numpy as np
import mediapipe as mp
import struct
import json

Debug = False
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_hands = mp.solutions.hands
stream = open(r'\\.\pipe\NPtest', 'r+b', 0)
i = 1
with mp_hands.Hands(
                min_detection_confidence=0.5,
                min_tracking_confidence=0.5) as hands: 
    while True:
        width = struct.unpack('I', stream.read(4))[0]   #Pipeline input 
        height = struct.unpack('I', stream.read(4))[0]   #Pipeline input   
        videoBytes = stream.read(width*height*3)   #Pipeline input       
        
        print(width)  
        print(height)


                           
        image = np.frombuffer(videoBytes, dtype=np.uint8)
        image = np.reshape(image, (height, width,3))
        
        handNumber=0

        results = hands.process(image) #calculation of images

        # Draw the hand annotations on the image.
        #image.flags.writeable = True
        
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        handData =[]
        hasResults = False
        if results.multi_hand_landmarks:
            hasResults = True
            hand = results.multi_hand_landmarks[handNumber] #results.multi_hand_landmarks returns landMarks for all the hands
            image
            for id, landMark in enumerate(hand.landmark):
                pixelLandmarkX = round(landMark.x*width)
                pixelLandmarkY = round(landMark.y*height)
                jsonData = {'id': id,'x': pixelLandmarkX ,'y':pixelLandmarkY, 'relZ': landMark.z} #JSON for Pipeline output
                handData.append(jsonData)

            if Debug: #DebugWindow -----v
                for hand_landmarks in results.multi_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image,
                        hand_landmarks,
                        mp_hands.HAND_CONNECTIONS,
                        mp_drawing_styles.get_default_hand_landmarks_style(),
                        mp_drawing_styles.get_default_hand_connections_style())
        
        dataListJson = json.dumps(handData)
        
        send = dataListJson.encode('ascii')
        
        i += 1
        if hasResults:
            stream.write(struct.pack('I', len(send)) + send)
        else:
            stream.write(struct.pack('I', 0))   # Write str length and str
        stream.seek(0)                               # EDIT: This is also necessary
        print('Wrote:', send)
        if Debug:
            imageShow = cv2.flip(image, 1)
            cv2.imshow('MediaPipe Hands', imageShow)
            cv2.waitKey(1)
            
        