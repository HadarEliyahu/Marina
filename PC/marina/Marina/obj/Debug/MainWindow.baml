   M S B A M L   `   `   `  ����
  Marina    Marina.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   \ XHardcodet.Wpf.TaskbarNotification, Version=1.0.5.0, Culture=neutral, PublicKeyToken=null)tb http://www.hardcodet.net/taskbar  5   	   -   5          � Title$  
MainWindow��5   	    � 
ResizeMode$ NoResize=�5   	    � AllowsTransparency .  � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$
 None=�5	   	   $��0q�5
   	    � Icon$ /Images/logo.png��5   	   $��Century Gothic)�5   	   
������  � 	Resources 5         MyImage  ConnectedDevices �� 	UriSource  Copy  	ShareFile  Message  	ShareLink  ListView��%�  &	        &	 0     &	 [     &	 �     &	 �     &	 �     &	     �� 5   
   $��./Images/send_icon.png��6    �� 5   
   $ ./Images/List.png?�6/   �� 5   
   $ ./Images/Copy.png?�6#   �� 5   
   $ ./Images/ShareFile.png?�6(   �� 5   
   $ ./Images/Message.png?�6&   �� 5   
   $ ./Images/ShareLink.png?�6(   �� 5   
   $ ./Images/ListViewIcon.png?�6'   5      .��5      �� ���#1d91da$��1q�6#   ���#212121._�5      �  � RowDefinitions 5#   
   �� 5$      $��2* �6   6*   �� 5%      $	��24* �6   6+   �� 5&      $��2* �6   6*   �� 5'      $��2* �6   6*   .+�5+   
   � ���#1d91da$	��Top=�6%   	 � ColumnDefinitions	 5,      �� 5-      $��2* �6#   �� 5.      $��1* �6#   �� 5/      $��3* �6#   �� 50      $��1* �6#   �� 51      $��1* �6#   �� 52      $��1* �6#   .+�55      �� -   5        
 ��Name$
 SignOutButton��55   o     	SignInTop#��[ 6   $��Sign Out��68   $y�Hidden=�6K   $��0��6_   6�   �� 56      -   5        $
 SignInButton��56   t   #��[ 6   $��Sign In��68   $��0��6d   6t   �� 5:      $��2��6   $��Marina��6(   $��35(�66   $��Center=�6D   $��Center=�6_   
�������6�   �� 5<      -   5          MinimizeButton#��[ 5<      $��_��6=   $��4��6I   6Y   �� 5=      -   5        $
 close��5=   c    	 XButton#��[	 6   $��X��66   $��5��6S   6c   5>      �� 5C   
   -   5         ��Name$ scrollMessages��5C   *   $��	0 10 0 10q�6   $
��Auto=�6B   $��
15 0 15 10q�6e   $��Stretch=�6z   $��1��6�   .��5D      �� -   5         ��Name$ messages��5D      $��Stretch=�6+   $��Stretch=�6I   6g   5E      � 5H      $��2��6   $��0 0 0 5q�6    ��5I      �� 5J       �� 
TargetType" �6   .�5K      ��  �� Property � �� �� Value �#1d91da67    �� Triggers 5L      �� 5M       �� Binding 6*   ��  �� ElementName$ 
sendButton�� �� Path$ IsMouseOver� �� Value True6g   .a�5N   "   ��  � �� �#0072ff6?   5O      �� 5P       6*   �� $ 
sendButton��$ 	IsEnabled�	 False6e   .a�5Q   "   ��  � �� �#cccccc6?   5R      5T      	 5V      �� 5W      $	��12* �6'   �� 5X      $��2* �6'   .+�5[      �� -   5         ��Name$ 
commandBox��5[   �   
�������$��Type your request��6,    
 TextBoxSample#��[
 6�   $��0��6�   6�   �� 5]      -	   5        $
 
sendButton��5]   �     ButtonSample#��[ 6   #��[  6?   $��1��6�   $
��Left=�6�   5^      � 5_      $��3��6   	 5`      �� 5a      $��4* �6'   �� 5b      $��2* �6'   �� 5c      $��2* �6'   .+�5f      ]� $��Center=�6   $��0��67   $��Center=�6G   .+�5g      �� -
   5         ��Name$ StartCheckBox��5g      $
��Left=�6m   $��Center=�6�   $
��Left=�6�   6�   �� 5h      $��10 0 0 0q�6    $��18(�62   
�������$��Right=�6S   Start on startup6|   5i      �� 5l      -   5          Icon#��[ 5l      $��1��67    � ToolTip$ Copy to phone clipboard��6G   ��5m      �� 5n      #��[ 6    5p      �� 5q      -   5        #��[ 5q      $��2��68   $ 
Share link��6H   ��5r      �� 5s      #��[ 6    5u      5v      5x      5y      5z      