   M S B A M L   `   `   `  ���� C  ?CharonUI, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:CharonUICharonUI  S OToggleSwitch, Version=1.1.2.0, Culture=neutral, PublicKeyToken=8637099990568f75A0clr-namespace:ToggleSwitch;assembly=ToggleSwitchToggleSwitch M6clr-namespace:ToggleSwitch.Utils;assembly=ToggleSwitchToggleSwitch.Utils Q8clr-namespace:ToggleSwitch.Borders;assembly=ToggleSwitchToggleSwitch.Borders     CharonUI.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     (localclr-namespace:CharonUI     IToggleSwitch0clr-namespace:ToggleSwitch;assembly=ToggleSwitch     HUtils6clr-namespace:ToggleSwitch.Utils;assembly=ToggleSwitch     LBorders8clr-namespace:ToggleSwitch.Borders;assembly=ToggleSwitch     $	��355��$	��580��  � WindowStyle$
  None=� � AllowsTransparency . 
������  � 	Resources    GroovedThumbTemplate �� 
TargetType'  !System.Windows.VisualStateManager   VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard g� Duration	 W� To	 �� TargetProperty
 �� 
TargetName*  $ToggleSwitch.Borders.OuterGlowBorder   OuterGlowSize   OuterGlowOpacity   OuterGlowColor   CornerRadius   ShadowCornerRadius)  #ToggleSwitch.Borders.ClippingBorder  Name   CornerRadius �� CornerRadius �� Padding �� Orientation J� Opacity ��Name J� IsHitTestVisible �� RadiusY �� RadiusX��%  &	        �� " n�.R�� 
������    CommonStates��.   
Normal��  	MouseOver��. �� .��W� $ 0D�$
 0.05X�$	 Opacity�$
 BackgroundAnimation��  Pressed��. �� .��W� $ 0D�$
 0.25X�$	 Opacity�$
 BackgroundAnimation��  Disabled��. �� .��W� $ 0D�$	 .55X�$	 Opacity�$
 DisabledVisualElement��  FocusStates��.   Focused��. �� .��W� $ 0D�$
 0.05X�$	 Opacity�$
 FocusVisualElement��  	Unfocused��  CheckStates��.   	Unchecked��  Checked��.+�  $ 4Q�$
 0.15Q�$��0q�$ Black��
������ $ 4��$ 8����.  .��� .+�  $ 
Background��#��z��#��z��$ 4����.  
�������.���� ���	#FFEBEBEB$ 3��#��z��#��z��$ 6q�._��� $ 
Horizontal=�$��Center=�.+��� ���	#7F969696$��1��$��0���� ���	#7FF8F8F8$��1��$��0��$��0,0,1,0q��� ���	#7F969696$��1��$��0���� ���	#7FF8F8F8$��1��$��0��$��0,0,1,0q��� ���	#7F969696$��1��$��0���� ���	#7FF8F8F8$��1��$��0��  $ BackgroundAnimation��
���   �$ 0Q�#��z��
���   �$ 4����.  �� $ DisabledVisualElement��
������� .  $ 0Q�$ 4��$ 4���� $ FocusVisualElement�� .  $��1q�$ 0Q�$ 4��$ 4��$��1��.���� ���	#FFA50505$��0,0,0,0q�$ 10,10,10,10��$��0,0,0,0q�._��� $��0,0,0,0q�.+��� -   $��522,10,0,10q�$��25��$��25��$��X�� J� RenderTransformOrigin$ 1.6,0.4&��� $��	35,5,35,0q�$ 5q����	#FF34302F$��3,5,3,5q�$ 10��$	��Top=�$	��270������  �� ImageSource$ 
bk-gnd.bmp��._�� $��8,5,2,1q� � ColumnDefinitions �� $
��162* ��� $
��218* ��� $
��105* � � RowDefinitions �� $	��30* ��� $	��35* ��� $	��30* ��� $	��30* ��� $	��35* ��� $	��40* ��� $	��44* �.+��� -    ��Name$ lblDate��$��0,9,1,0q�&   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�$��Right=�$	��Top=�$
��Bold"�$	��240��$��1��$��0��
�������$��17��$��2���� -     ��Name$  lblSunriseVal��$��07:05 AM��$��2��$��Right=�$��1��$��70��$��24�����	#FFFAFAFA$	��Top=�$	��5,2q�$��0,7,33,0q�$
��Bold"��� -   $  
lblSunrise��$��Today's Sunrise:��$��1��$��Right=�$��0,7,0,4q�$��1��$��Center=�$	��120��$��24�����	#FFFAFAFA$��5,2,5,5q�$
��Bold"��� -   $  	lblSunset��$��Today's Sunset:��$��1��$��Right=�$��0,1,1,0q�$��2��$	��Top=�$	��120��
�������$��26��$��5,2,5,5q�$
��Bold"��� -   $  lblSunsetVal��$��06:15 PM��$��2��$��Right=�$��0,1,34,0q�$��2��$	��Top=�$��70��$��24��
�������$	��5,2q�$
��Bold"�)  #ToggleSwitch.HorizontalToggleSwitch  -   !  Name$! BtnDCBus��#  ToggleSwitch.ToggleSwitchBase"   	IsChecked" . $
��Left=�$��2��$��5��$��9,12,0,4q�$��24��  -   $! BtnACBus��" . $
��Left=�$��2��$��6��$��9,4,0,16q�$��24���� -	   $  lblDCBus��$��DC Bus:��$��1��$��0,12,1,0q�$��5��$	��Top=�$��26��
�������$��0,5,0,0q�$
��Bold"�$��50��$��Right=��� -
   $  lblACBus��$��AC Bus:��$��1��$��Right=�$��0,1,1,0q�$��6��$	��Top=�$��50��$��26��$
��Bold"�
�������$��0,5,0,0q�