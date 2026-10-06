using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DesignStudio.Application.Documentation;
using DesignStudio.Application.Products;
using DesignStudio.Domain.Documentation;
using DesignStudio.Domain.Project;
using Microsoft.Win32;

namespace DesignStudio.UI;

public partial class ProductionDrawingWindow : System.Windows.Window
{
    private ProductionDrawingPage? _page;
    private string _svg = string.Empty;

    public ProductionDrawingWindow()
    {
        InitializeComponent();
        Loaded += ProductionDrawingWindow_Loaded;
    }

    private void ProductionDrawingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ProductionDrawingWindow_Loaded;

        try
        {
            BuildProductionPreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.ToString(),
                "Production Drawing Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void BuildProductionPreview()
    {
        var project = new DesignStudio.Domain.Project.Project("Production Preview");

        var created = new CabinetService().Create(
            project,
            shelfCount: 2,
            doorCount: 2);

        var cutList = new CabinetCutListService()
            .Build(created.Generation);

        var nesting = new CabinetNestingService()
            .Build(project, cutList);

        var documents = new CabinetProductionDocumentationService()
            .Build(
                project,
                created.Cabinet,
                created.Generation,
                cutList,
                nesting,
                revision: "A");

        _page = new CabinetProductionDrawingBuilder()
            .Build(documents);

        _svg = new SvgProductionDrawingExporter()
            .Export(_page);

        HeaderText.Text =
            $"{_page.Title}  •  {_page.DocumentCode}  •  REV {_page.Revision}";

        Render(_page);
    }

    private void Render(ProductionDrawingPage page)
    {
        DrawingCanvas.Children.Clear();

        foreach (var primitive in page.Primitives)
        {
            switch (primitive.Type)
            {
                case DrawingPrimitiveType.Line:
                {
                    var line = new Line
                    {
                        X1 = primitive.Start.X,
                        Y1 = primitive.Start.Y,
                        X2 = primitive.End.X,
                        Y2 = primitive.End.Y,
                        Stroke = Brushes.Black,
                        StrokeThickness = Math.Max(0.2, primitive.StrokeWidth)
                    };

                    DrawingCanvas.Children.Add(line);
                    break;
                }

                case DrawingPrimitiveType.Rectangle:
                {
                    var rectangle = new Rectangle
                    {
                        Width = Math.Max(0, primitive.End.X - primitive.Start.X),
                        Height = Math.Max(0, primitive.End.Y - primitive.Start.Y),
                        Fill = Brushes.Transparent,
                        Stroke = Brushes.Black,
                        StrokeThickness = Math.Max(0.2, primitive.StrokeWidth)
                    };

                    Canvas.SetLeft(rectangle, primitive.Start.X);
                    Canvas.SetTop(rectangle, primitive.Start.Y);

                    DrawingCanvas.Children.Add(rectangle);
                    break;
                }

                case DrawingPrimitiveType.Text:
                {
                    var text = new TextBlock
                    {
                        Text = primitive.Text,
                        FontFamily = new FontFamily("Arial"),
                        FontSize = Math.Max(1, primitive.FontSize),
                        Foreground = Brushes.Black,
                        TextWrapping = TextWrapping.NoWrap
                    };

                    Canvas.SetLeft(text, primitive.Start.X);
                    Canvas.SetTop(
                        text,
                        Math.Max(0, primitive.Start.Y - primitive.FontSize));

                    DrawingCanvas.Children.Add(text);
                    break;
                }
            }
        }
    }

    private void SaveSvg_Click(object sender, RoutedEventArgs e)
    {
        if (_page is null || string.IsNullOrWhiteSpace(_svg))
        {
            MessageBox.Show(
                this,
                "No production drawing has been generated.",
                "Production Drawing",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Save Production Drawing",
            Filter = "SVG Drawing (*.svg)|*.svg|All Files (*.*)|*.*",
            FileName = $"{_page.DocumentCode}-REV-{_page.Revision}.svg",
            AddExtension = true
        };

        if (dialog.ShowDialog(this) != true)
            return;

        File.WriteAllText(
            dialog.FileName,
            _svg,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        MessageBox.Show(
            this,
            $"SVG saved successfully:\n{dialog.FileName}",
            "Production Drawing",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
