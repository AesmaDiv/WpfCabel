   M S B A M L   `   `   `  ����C  ?xLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5(clr-namespace:xLibrary;assembly=xLibraryxLibrary  5      E AWPF_Remake, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:WPF_TryWPF_Try 5         WPF_Try.SettingPage   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      =xLibrary(clr-namespace:xLibrary;assembly=xLibrary     5      &mainclr-namespace:WPF_Try     5      $	��480��5	      $	��670��6     N� Title$  SettingPage��5
      .��5      � .+�5   
   �� ��5      �� 5       �� ImageSource$&  Resources/Images/bckblur_rev.png��6   ._�5      �� $��НАСТРОЙКИ��6   $
��Left=�6*   $��	10,22,0,0q�6E   $	��Top=�6X   $	��448��6p   $	��650��6}   ���	#FFFFFCFC J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5      .��5      � .+�5      �� -   5         ��Name$ bdrClose��5      $��20��6/   $��15��6:   $��Right=�6F   $	��Top=�6b   $��
0,-30,-5,0q�6z    �� CornerRadius$ 3��6�   $��2q�6�   
���������5      �� 5      $$ Resources/Images/btn_no_bw.png��6)   5      �� 5      $��5q�67   ��6!   Z� .@�5      �� $/��)Параметры подключений��6"      TabItemStyle#���   6A   .��5      ��  �� Orientation$ 
Horizontal=�6)   $��10q�6B   $��Right=�6N   .+�5    "   �� $ Vertical=�6-   .+�5!   &   �� $ 
Horizontal=�61   $��5q�6J   $��Right=�6U   .+�5"   *   �� $��Right=�64   $
��Wrap=�6Q   $��Путь к Access БД:��6e   $��Center=�6~    �� Padding$ 5q�6�   
�������6�   �� 5#   *   -   5         ��Name$ txtAccess_Path��5#   2   $��Right=�6d   $��23��6�    �� TextWrapping$
 Wrap=�6�   $�� ��6�   $��Center=�6�   $	��180��6�   6�   �� 5$   *   -   5        	 ��Name$	 btnAccess_Path��5$   1   $��20��6c   $	��...��6n   6|   5%   '   �� 5&   &   $ 
Horizontal=�61   $��5q�6J   $��Right=�6U   .+�5'   *   �� $��Right=�64   $
��Wrap=�6P   $!��Имя таблицы БД:��6d   $��Center=�6{   $ 5q�6�   
�������6�   �� 5(   *   -   5        $ txtAccess_Table��5(   2   $��Right=�6e   $��23��6�   $
 Wrap=�6�   $�� ��6�   $��Center=�6�   $	��140��6�   6�   5)   '   �� 5*   &   $ 
Horizontal=�61   $��5q�6J   $��Right=�6U   �� 5+   &   $ 
Horizontal=�61   $��5q�6J   $��Right=�6U   .+�5,   *   �� $��Right=�64   $
��Wrap=�6Q   $��Di30R COM-Порт:��6e   $��Center=�6|   $ 5q�6�   
�������6�   �� 5-   *   -   5        
 ��Name$
 cbCOM_Di30r��5-   3   $	��100��6H   $��23��6T   5.   '   �� 5/   &   $ 
Horizontal=�61   $��5q�6J   $��Right=�6U   .+�50   *   �� $��Right=�64   $
��Wrap=�6Q   $��CA6250 COM-Порт:��6e   $��Center=�6}   $ 5q�6�   
�������6�   �� 51   *   -   5        $
 cbCOM_CA6250��51   3   $	��100��6I   $��23��6U   52   '   53   #   54      55      �� 56      $:��4Калибровочные коэффициенты:��6"   .��57      �� $ Vertical=�6)   $��10q�6@   .+�58   "      xLibrary.xCustomTable  -   5          Name$ tableCoeffs��58   8   $
��Left=�6M   $	��Top=�6h   $	��600��6�      RowCount$ 2��6�      ColumnCount$ 4��6�      DataColumnWidth$	 140Q�59   8      HeaderColumnWidth$ 40Q�6N      CellBackground �	#FFCECECE   HasRowHeader .    HeaderBackground �	#FF969696   CellForeground �	#FF040404   CellBorderbrush
 �   �   IsCellsEditable .    ColumnsTypeArray$ 1,1,1,1,1,1��5:   g      CellBorderThickness$	 0.2q�6�   �� 5;   "   $ 
Horizontal=�6-   $
��Left=�6G   $	��Top=�6b   $��5,10,0,0q�6z   .+�5<   &   �� $j��dВремя стабилизации при высововольтном испытании (сек):��60   $	��332��6n   
�������$��Center=�6�   �� 5=   &   -   5        $ txtStabilizationTime��5=   .   $��50��6L   $��10,0,0,0q�6W   5>   #   5@      5A      �� 5B      $��Adam5000TCP��6"   .��5C      �� $��10q�6)   .+�5D   "   �� $ 
Horizontal=�6-   $��5q�6F   $��Right=�6Q   $	��Top=�6m   .+�5E   &   �� $��Right=�60   $
��Wrap=�6M   $��Adam IP:��6a   $��Center=�6q   $ 5q�6�   
�������6�   �� 5F   &   -	   5        $ 	txtAdamIP��5F   .   $��Right=�6[   $��23��6w   $
 Wrap=�6�   $�� ��6�   $��Center=�6�   $	��200��6�   6�   5G   #     5H   "   -
   5        $ tableAdamInfo��5H   8   $
��Left=�6O   $	��Top=�6j   $ 2��6�   $ 6��6�   $ 90Q�5I   8   $ 67Q�6M    �	#FFCECECE .  �	#FF969696 �	#FF040404$��0,5,0,0q�5J   8   
 �   � . $ 1,1,1,1,1,1��6x   $	 0.2q�6�   5K      5O      5P      �� 5R      -   5        $ bdrSave��5R      $	��120��6.   $��30��6:   
�������$��Right=�6Z   $��Bottom=�6v   $��	0,0,10,10q�6�   $��2q�6�   $ 4��6�   ��5S      �� 5T      $&  Resources/Images/btn_Save_bw.png��6)   5V      5X      5Y      5\      5]      5_      