   M S B A M L   `   `   `  ����C  ?RealHand, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:RealHandRealHand  5   	       RealHand.DebugWindow   5   	   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:RealHand     5   	     � Title$  
MainWindow��5   	   $
��1080��6   $
��1920��6*   .��5	      �  � ColumnDefinitions 5
   
   �� 5      �� 5       � RowDefinitions 5   
   �� 5      $��5* �6   �� 5      $��3* �6   .+�5   
   �� -   5         ��Name$ imgDepth��5      $��0��6"   $��0��62   $��0,33,0,8q�6?   �� 5   
   -   5        $ imgColor��5      $��1��6"   $��0��62   � 5   
   $��1��6   $��	0,537,0,0q�6   $��2��62    5      �� 5      $	��130 �6#   �� 5       5      �� 5      $
��128* �6    �� 5      $
��135* �6    �� 5      $
��264* �6    .+�5      �� $��0��6   $��1��6)   $��0,4,0,21q�66   $��2��6H   �� 5       �� 5!      $�Wrist (0): ��6   
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5"      �� 5#      $�Thumb_CMC (1): ��6   
���   �$��Arial)�6@   $��10(�6S   $
��Bold"�6a   �� 5$      �� 5%      $�Thumb_MCP (2): ��6   
���   �$��Arial)�6@   $��10(�6S   $
��Bold"�6a   �� 5&      �� 5'      $�Thumb_IP (3)��6   
���   �$��Arial)�6=   $��10(�6P   $
��Bold"�6^   �� 5(      �� 5)      $�Thumb_TIP (4): ��6   
���   �$��Arial)�6@   $��10(�6S   $
��Bold"�6a   �� 5*      �� 5+      $�Index_Finger_MCP (5): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 5,      �� 5-      $�Index_Finger_PIP (6): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 5.      �� 5/      $�Index_Finger_DIP (7): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 50      �� 51      $�Index_Finger_TIP (8): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 52      �� 53      $�Middle_Finger_MCP (9): ��6   
���   �$��Arial)�6H   $��10(�6[   $
��Bold"�6i   �� 54      �� 55      $�Middle_Finger_PIP (10): ��6   
���   �$��Arial)�6I   $��10(�6\   $
��Bold"�6j   �� 56      �� 57      $�Middle_Finger_DIP (11): ��6   
���   �$��Arial)�6I   $��10(�6\   $
��Bold"�6j   �� 58      �� 59      $�Middle_Finger_TIP (12): ��6   
���   �$��Arial)�6I   $��10(�6\   $
��Bold"�6j   �� 5:      �� 5;      $�Ring_Finger_MCP (13): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 5<      �� 5=      $�Ring_Finger_PIP (14): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 5>      �� 5?      $�Ring_Finger_DIP (15): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 5@      �� 5A      $�Ring_Finger_TIP (16): ��6   
���   �$��Arial)�6G   $��10(�6Z   $
��Bold"�6h   �� 5B      �� 5C      $�Pinky_MCP (17): ��6   
���   �$��Arial)�6A   $��10(�6T   $
��Bold"�6b   �� 5D      �� 5E      $�Pinky_PIP (18): ��6   
���   �$��Arial)�6A   $��10(�6T   $
��Bold"�6b   �� 5F      �� 5G      $�Pinky_DIP (19): ��6   
���   �$��Arial)�6A   $��10(�6T   $
��Bold"�6b   �� 5H      �� 5I      $�Pinky_TIP (20): ��6   
���   �$��Arial)�6A   $��10(�6T   $
��Bold"�6b   �� 5J      �� 5K      $�Startup Time: ��6   
���   �$��Arial)�6?   $��10(�6R   $
��Bold"�6`   �� 5L      �� 5M      $�Iteration time: ��6   
���   �$��Arial)�6A   $��10(�6T   $
��Bold"�6b   5O      �� 5P      $��1��6   $��1��6)   $��0,4,0,21q�66   $��2��6H   �� 5Q      �� 5R      -   5         ��Name$ wrist��5R      
���   �$��Arial)�68   $��10(�6K   $
��Bold"�6Y   �� 5S      �� 5T      -   5        $ 	thumb_CMC��5T      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5U      �� 5V      -   5        $ 	thumb_MCP��5V      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5W      �� 5X      -   5        $ thumb_IP��5X      
���   �$��Arial)�6;   $��10(�6N   $
��Bold"�6\   �� 5Y      �� 5Z      -   5        $ 	thumb_TIP��5Z      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5[      �� 5\      -   5        $ index_Finger_MCP��5\      
���   �$��Arial)�6C   $��10(�6V   $
��Bold"�6d   �� 5]      �� 5^      -	   5        $ index_Finger_PIP��5^      
���   �$��Arial)�6C   $��10(�6V   $
��Bold"�6d   �� 5_      �� 5`      -
   5        $ index_Finger_DIP��5`      
���   �$��Arial)�6C   $��10(�6V   $
��Bold"�6d   �� 5a      �� 5b      -   5        $ index_Finger_TIP��5b      
���   �$��Arial)�6C   $��10(�6V   $
��Bold"�6d   �� 5c      �� 5d      -   5        $ middle_Finger_MCP��5d      
���   �$��Arial)�6D   $��10(�6W   $
��Bold"�6e   �� 5e      �� 5f      -   5        $ middle_Finger_PIP��5f      
���   �$��Arial)�6D   $��10(�6W   $
��Bold"�6e   �� 5g      �� 5h      -   5        $ middle_Finger_DIP��5h      
���   �$��Arial)�6D   $��10(�6W   $
��Bold"�6e   �� 5i      �� 5j      -   5        $ middle_Finger_TIP��5j      
���   �$��Arial)�6D   $��10(�6W   $
��Bold"�6e   �� 5k      �� 5l      -   5        $ ring_Finger_MCP��5l      
���   �$��Arial)�6B   $��10(�6U   $
��Bold"�6c   �� 5m      �� 5n      -   5        $ ring_Finger_PIP��5n      
���   �$��Arial)�6B   $��10(�6U   $
��Bold"�6c   �� 5o      �� 5p      -   5        $ ring_Finger_DIP��5p      
���   �$��Arial)�6B   $��10(�6U   $
��Bold"�6c   �� 5q      �� 5r      -   5        $ ring_Finger_TIP��5r      
���   �$��Arial)�6B   $��10(�6U   $
��Bold"�6c   �� 5s      �� 5t      -   5        $ 	pinky_MCP��5t      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5u      �� 5v      -   5        $ 	pinky_PIP��5v      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5w      �� 5x      -   5        $ 	pinky_DIP��5x      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5y      �� 5z      -   5        $ 	pinky_TIP��5z      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5{      �� 5|      -   5        $ 	firstTime��5|      
���   �$��Arial)�6<   $��10(�6O   $
��Bold"�6]   �� 5}      �� 5~      -   5        $ iterationTime��5~      
���   �$��Arial)�6@   $��10(�6S   $
��Bold"�6a   5      5�      5�      5�      