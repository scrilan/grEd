﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace grEd
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public partial class MainWindow
	{
		private readonly FiguresList figuresList;
		public List<Figure.Figure> figuresOnCanvas { get; set; }
		public Figure.Figure selectedFigure { get; set; }

		public MainWindow()
		{
			InitializeComponent();
//			loadLibraries(@"D:\for-study\ootpisp\labs\01\grEd\grEd\bin\Debug\bin\DefaultFigures.dll");
//			loadLibraries("/bin");

			figuresList = new FiguresList(Canvas);
			figuresOnCanvas = figuresList.figures;

			comboBoxesInitialization();
		}


		private void loadLibraries(string path)
		{
			Assembly assembly = Assembly.Load(path);

			availableFiguresToDraw = new List<Type>();

			availableFiguresToDraw.AddRange(assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Figure.Figure))));
		}

		






		private void comboBoxesInitialization()
		{
			colorsListInitialization();
			thicknessesListInitialization();
			fillRulesInitialization();
//			availableFiguresBoxInitialization();
			DataContext = this;

			selectedStrokeColor = Brushes.Black;
			selectedFillColor = Brushes.Transparent;
			selectedFillRule = FillRule.Nonzero;
			selectedStrokeThickness = 1;
		}

		public List<SolidColorBrush> colorsList { get; set; }
		public List<int> thicknessesList { get; set; }
		public List<FillRule> fillRules { get; set; }
		public SolidColorBrush selectedStrokeColor { get; set; }
		public SolidColorBrush selectedFillColor { get; set; }
		public FillRule selectedFillRule { get; set; }
		public int selectedStrokeThickness { get; set; }
		public List<Type> availableFiguresToDraw { get; set; }
		public Type selectedFigureTypeToDraw { get; set; }
		private void colorsListInitialization()
		{
			colorsList = new List<SolidColorBrush>();

			Type brushesType = typeof(Brushes);
			// Get all static properties
			var properties = brushesType.GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (var prop in properties)
			{
//				string name = prop.Name;
				SolidColorBrush brush = (SolidColorBrush)prop.GetValue(null, null);
				colorsList.Add(brush);
			}
		}
		private void thicknessesListInitialization()
		{
			const int maxThickness = 30;
			thicknessesList = new List<int>();
			for (int i = 1; i < maxThickness; i++)
				thicknessesList.Add(i);
		}
		private void fillRulesInitialization()
		{
			fillRules = new List<FillRule> {FillRule.EvenOdd, FillRule.Nonzero};
		}
		private void availableFiguresBoxInitialization()
		{
			availableFiguresToDraw = new List<Type>();
			//добавляемые фигуры могут наследоваться только от класса Figure (походу и от его производных тоже)
			availableFiguresToDraw.AddRange(AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsSubclassOf(typeof (Figure.Figure))));
		}


		



		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}


		private void strokeColorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (selectedFigure != null)
			{
				selectedFigure.stroke = selectedStrokeColor;
			}
		}
		private void strokeThicknessBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (selectedFigure != null)
			{
				selectedFigure.strokeThickness = selectedStrokeThickness;
			}
		}
		private void fillColorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (selectedFigure != null)
			{
				selectedFigure.fill = selectedFillColor;
			}
		}
		private void fillRuleBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (selectedFigure != null)
			{
				selectedFigure.fillRule = selectedFillRule;
			}
		}


		private void removeButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedFigure != null)
			{
				figuresList.remove(selectedFigure);
				//костыль, без которого не обновляется ListBox:
				FiguresOnCanvasBox.Items.Refresh();	
			}
		}
		private void FiguresOnCanvasBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				removeButton_Click(sender, e);
			}
		}


		private dynamic currentDrawingFigure;
		private void drawButton_Click(object sender, RoutedEventArgs e)
		{
			dynamic figure = Activator.CreateInstance(selectedFigureTypeToDraw);

			figure.stroke = selectedStrokeColor;
			figure.fill = selectedFillColor;
			figure.strokeThickness = selectedStrokeThickness;
			figure.fillRule = selectedFillRule;

			figuresList.add(figure as Figure.Figure);
			FiguresOnCanvasBox.Items.Refresh();	

			currentDrawingFigure = figure;
		}


		private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (isFigureBeingDrawedAndExists())
			{
				currentDrawingFigure.mouseDrawHandler(Mouse.GetPosition(Canvas));
			}
			else
			{
				drawButton_Click(sender, e);
				currentDrawingFigure.mouseDrawHandler(Mouse.GetPosition(Canvas));
			}
		}

		private bool isFigureBeingDrawedAndExists()
		{
			return ((currentDrawingFigure != null) && (!currentDrawingFigure.isFigureFinished()));
		}

		private void Canvas_MouseMove(object sender, MouseEventArgs e)
		{
			if (isFigureBeingDrawedAndExists())
			{
				currentDrawingFigure.mousePreviewHandler(Mouse.GetPosition(Canvas));
			}
		}

		private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (isFigureBeingDrawedAndExists())
			{
				currentDrawingFigure.stopDrawing();
			}
		}

		private void clearButton_Click(object sender, RoutedEventArgs e)
		{
			figuresList.clear();
			FiguresOnCanvasBox.Items.Refresh();
		}


	
	}
}
