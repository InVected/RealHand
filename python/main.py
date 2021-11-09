
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
        width = struct.unpack('I', stream.read(4))[0]
        height = struct.unpack('I', stream.read(4))[0]     
        videoBytes = stream.read(width*height*3)          
        
        print(width)  
        print(height)


                            # Important!!!
        image = np.frombuffer(videoBytes, dtype=np.uint8)
        image = np.reshape(image, (height, width,3))
        
        #results = hands.process(image)
        handNumber=0
        #image.flags.writeable = False
        
        #image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = hands.process(image)

        # Draw the hand annotations on the image.
        #image.flags.writeable = True
        
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        handData =[]
        hasResults = False
        if results.multi_hand_landmarks:
            hasResults = True
            hand = results.multi_hand_landmarks[handNumber] #results.multi_hand_landmarks returns landMarks for all the hands
            for id, landMark in enumerate(hand.landmark):
                jsonData = {'id': id,'x': landMark.x ,'y':landMark.y, 'z': landMark.z}
                handData.append(jsonData)

            if Debug:
                for hand_landmarks in results.multi_hand_landmarks:
                    mp_drawing.draw_landmarks(
                        image,
                        hand_landmarks,
                        mp_hands.HAND_CONNECTIONS,
                        mp_drawing_styles.get_default_hand_landmarks_style(),
                        mp_drawing_styles.get_default_hand_connections_style())
        
        #sendList = {'hasResults':hasResults, 'data': handData}
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
        #byte = s.read(640*360*3)
            
        