   M S B A M L   `   `   `  ����  MySchoolYear#    MySchoolYear.View.LoginWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5          �Name$  ThisLoginWindow��5   	    � Title$ MySchoolYear��5   	   $	��300��6   $	��500��6+      WindowBackgroundBrush#��[  5	   	   .��5
      �  +� FocusedElement 6   ��  �� ElementName$ usernameTextBox�� � RowDefinitions 5   
   �� 5      $
��1.5* �6   �� 5      $��1* �6   .+�5   
   �� $��0��6   $2��,/MySchoolYear;component/Images/LogoImage.png��6   $
��Fill=�6S   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $��1* �6#   �� 5      $��1* �6#   .+�5      �� -   5        $��usernameTextBox��5      $��0��6-   $��0��6:   $��Center=�6J    �� 	MaxLength$ 16��6e     placeHolderTextBox#��[ 5      
 � Tag$ שם משתמש...��6B   $	��200��5      $��0,0,0,40q�6"   ��5   T   ��  �� Path$ Username�5   "   G� 5      -   5        	 G�Name$	 PasswordText��5      $��1��60   $��0��6=   $��Center=�6M   $ סיסמא...��6h   
 G� 	MaxLength$
 16��5      $	��200��6*   
���   �$��0,40,0,0q�6J   �� 5      $��1��6   $��
התחבר��6%   $��Center=�65    �� 	IsDefault . ��5      �� $ LoginCommand���6;   �� $ ThisLoginWindow��5      5      5       