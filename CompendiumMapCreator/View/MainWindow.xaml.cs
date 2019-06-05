using CompendiumMapCreator.Controls;
using CompendiumMapCreator.Data;
using CompendiumMapCreator.View;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CompendiumMapCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DragHelper drag;
        private ImagePoint change = new ImagePoint(0, 0);
        private IList<Element> copied;
        private bool justClosed;
        private bool moving;
        private ImagePoint origin;

        public MainWindow()
        {
            this.InitializeComponent();

            //this.SelectTool(Tools.Cursor);

            this.ViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(this.ViewModel.Project))
                {
                    this.Zoom.Center();
                }
                else if (e.PropertyName == nameof(this.ViewModel.SelectedTool))
                {
                    this.SelectTool(this.ViewModel.SelectedTool);
                }
            };

            this.drag = new DragHelper(this.ViewModel);
        }

        public ViewModel.MainWindow ViewModel => (ViewModel.MainWindow)this.DataContext;

        private void AboutWindow(object sender, RoutedEventArgs e) => new AboutWindow() { Owner = this }.Show();

        private void AddTitle_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.Project == null)
            {
                return;
            }

            TitleWindow window = new TitleWindow(this.ViewModel.Project.Title)
            {
                Owner = this,
            };

            bool? result = window.ShowDialog();

            if (result.GetValueOrDefault())
            {
                this.ViewModel.SetTitle(window.MapTitle);
            }
        }

        private void ChangeMap_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ChangeImage();
        }

        private void Copy()
        {
            if (this.ViewModel.Project == null)
            {
                return;
            }
            this.copied = new List<Element>(this.ViewModel.Project.Selected);
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            this.Copy();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Delete();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.Project.Selected.Count != 1)
            {
                return;
            }

            Element element = this.ViewModel.Project.Selected[0];

            this.ViewModel.Edit((element.Position() + new ImagePoint(element.Width / 2, element.Height / 2)).ToWindow(this.Zoom) + new WindowPoint(160, 20));

            this.EditWindow.Focus();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Export();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.LoadImage(this.Window);
        }

        private void LoadProject_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.LoadProject(this.Window);
        }

        private void Paste()
        {
            if (this.copied != null)
            {
                ImagePoint p = Mouse.GetPosition(this.Zoom).ToImage(this.Zoom);

                this.ViewModel.Project.Copy(p, this.copied);
            }
        }

        private void Paste(object sender, RoutedEventArgs e)
        {
            this.Paste();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Redo();
        }

        private void RotateClockwise(object sender, RoutedEventArgs e) => this.ViewModel.RotateClockwise();

        private void RotateCounterClockwise(object sender, RoutedEventArgs e) => this.ViewModel.RotateCounterClockwise();

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.SaveProject();
        }

        private void SaveProjectAs_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.SaveProject(forcePrompt: true);
        }

        private void SelectTool(Tool tool)
        {
            bool Select(TreeViewItem item)
            {
                if (((Tool)item.Header).Description == tool.Description)
                {
                    item.IsSelected = true;
                    return true;
                }

                bool isExpanded = item.IsExpanded;
                if (!isExpanded)
                {
                    item.IsExpanded = true;
                    item.UpdateLayout();
                }

                for (int i = 0; i < item.Items.Count; i++)
                {
                    TreeViewItem sub = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);

                    if (Select(sub))
                    {
                        return true;
                    }
                }

                item.IsExpanded = isExpanded;
                return false;
            }

            for (int i = 0; i < this.ToolsView.Items.Count; i++)
            {
                TreeViewItem item = (TreeViewItem)this.ToolsView.ItemContainerGenerator.ContainerFromIndex(i);

                if (Select(item))
                {
                    return;
                }
            }
        }

        private void ShowShortcuts(object sender, RoutedEventArgs e) => new ShortcutsWindow() { Owner = this }.Show();

        private void Tools_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Tool tool = (Tool)this.ToolsView.SelectedValue;

            if (tool.IsSelectable)
            {
                this.ViewModel.SelectedTool = tool;
            }
            else
            {
                this.ViewModel.SelectedTool = tool.Tools[0];
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Undo();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => e.Cancel = this.ViewModel.Changing(System.Windows.Window.GetWindow(this.Zoom));

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.ViewModel.Editing.Visibility != Visibility.Collapsed && e.Key == Key.Escape)
            {
                this.ViewModel.Editing.End();
            }

            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 && (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0)
            {
                switch (e.Key)
                {
                    //Save project as
                    case Key.S:
                        this.ViewModel.SaveProject(forcePrompt: true);
                        return;
                }
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            {
                switch (e.Key)
                {
                    //Load Image
                    case Key.N:
                        this.ViewModel.LoadImage(this.Window);
                        return;
                    //Export
                    case Key.E:
                        this.ViewModel.Export();
                        return;
                    //Undo
                    case Key.Z:
                        this.ViewModel.Undo();
                        return;
                    //Redo
                    case Key.Y:
                        this.ViewModel.Redo();
                        return;
                    //Save project
                    case Key.S:
                        this.ViewModel.SaveProject();
                        return;
                    //Load project
                    case Key.L:
                        this.ViewModel.LoadProject(this.Window);
                        return;
                    //Copy
                    case Key.C:
                        this.Copy();
                        return;
                    //Paste
                    case Key.V:
                        this.Paste();
                        return;
                    //Deselect
                    case Key.D:
                        this.ViewModel.Deselect();
                        return;

                    case Key.A:
                        this.ViewModel.SelectAll();
                        return;
                }
            }
            else if (this.ViewModel.Editing.Visibility == Visibility.Collapsed)
            {
                switch (e.Key)
                {
                    //Cursor
                    case Key.D1:
                        this.ViewModel.SetTool(Tools.Cursor);
                        return;

                    case Key.R:
                        this.ViewModel.SetTool(Tools.Rewards.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.C:
                        this.ViewModel.SetTool(Tools.Collectible.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.D:
                        this.ViewModel.SetTool(Tools.Door.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.T:
                        this.ViewModel.SetTool(Tools.Traps.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.A:
                        this.ViewModel.SetTool(Tools.Activators.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.Q:
                        this.ViewModel.SetTool(Tools.QuestItems.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.E:
                        this.ViewModel.SetTool(Tools.Movement.Next(this.ViewModel.SelectedTool));
                        return;

                    case Key.W:
                        this.ViewModel.SetTool(Tools.Workstation.Next(this.ViewModel.SelectedTool));
                        return;
                }
            }

            switch (e.Key)
            {
                //Delete
                case Key.Delete:
                    this.ViewModel.Delete();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.SelectTool(Tools.Cursor);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.justClosed = false;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ViewModel.Editing.Visibility == Visibility.Visible)
            {
                Point p = e.GetPosition(this.EditWindow);

                if (p.X < 0 || p.Y < 0 || p.X > 200 || p.Y > 50)
                {
                    this.ViewModel.Editing.End();
                    this.justClosed = true;
                }
            }
        }

        private void Zoom_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.ViewModel.Project == null)
            {
                return;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                foreach (Element element in this.ViewModel.Project.Selected)
                {
                    element.Opacity = 0.25;
                }
            }

            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                if (!this.moving && this.ViewModel.Project.Selected.Count > 0)
                {
                    this.moving = true;
                    this.origin = new ImagePoint(this.ViewModel.Project.Selected[0].X, this.ViewModel.Project.Selected[0].Y);
                    this.ViewModel.DragStart(this.origin);

                    if (Keyboard.IsKeyDown(Key.Up))
                    {
                        this.change.Y--;
                    }

                    if (Keyboard.IsKeyDown(Key.Down))
                    {
                        this.change.Y++;
                    }

                    if (Keyboard.IsKeyDown(Key.Left))
                    {
                        this.change.X--;
                    }

                    if (Keyboard.IsKeyDown(Key.Right))
                    {
                        this.change.X++;
                    }
                    this.ViewModel.DragUpdate(this.origin + this.change);
                }
                else if (this.moving)
                {
                    if (Keyboard.IsKeyDown(Key.Up))
                    {
                        this.change.Y--;
                    }

                    if (Keyboard.IsKeyDown(Key.Down))
                    {
                        this.change.Y++;
                    }

                    if (Keyboard.IsKeyDown(Key.Left))
                    {
                        this.change.X--;
                    }

                    if (Keyboard.IsKeyDown(Key.Right))
                    {
                        this.change.X++;
                    }

                    this.ViewModel.DragUpdate(this.origin + this.change);
                }
            }
        }

        private void Zoom_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.ViewModel.Project == null)
            {
                return;
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                foreach (Element element in this.ViewModel.Project.Selected)
                {
                    element.Opacity = 1;
                }
            }

            if (this.moving && Keyboard.IsKeyUp(Key.Up) && Keyboard.IsKeyUp(Key.Down) && Keyboard.IsKeyUp(Key.Left) && Keyboard.IsKeyUp(Key.Right))
            {
                this.ViewModel.DragEnd();
                this.change = new ImagePoint(0, 0);

                this.moving = false;
            }
        }

        private void Zoom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.justClosed)
            {
                return;
            }

            ((ZoomControl)sender).Focus();

            this.drag.MouseDown(e.GetPosition(this.Zoom).AsWindow());
        }

        private void Zoom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.drag.MouseUp(e.GetPosition(this.Zoom).ToImage(this.Zoom));
        }

        private void Zoom_MouseMove(object sender, MouseEventArgs e) => this.drag.MouseMove(e.GetPosition(this.Zoom).ToImage(this.Zoom), e.GetPosition(this.Zoom).AsWindow(), e.LeftButton, this.Zoom);

        private void Zoom_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => this.ViewModel?.Project?.Select(e.GetPosition(this.Zoom).ToImage(this.Zoom));

        private void Zoom_ScaleChanged(object sender, EventArgs e)
        {
            this.ViewModel.Editing.End();
        }

        private void Zoom_ViewportChanged(object sender, EventArgs e)
        {
            this.ViewModel.Editing.End();
        }

        private class DragHelper
        {
            private readonly ViewModel.MainWindow viewModel;
            private bool dragging;
            private bool mouseDown;
            private WindowPoint start;

            public DragHelper(ViewModel.MainWindow viewModel)
            {
                this.viewModel = viewModel;
            }

            public void MouseDown(WindowPoint p)
            {
                this.mouseDown = true;
                this.start = p;
            }

            public void MouseMove(ImagePoint p, WindowPoint wp, MouseButtonState state, ZoomControl zoom)
            {
                WindowPoint diff = this.start - wp;

                if (this.mouseDown && state == MouseButtonState.Released)
                {
                    this.MouseUp(p);
                }
                else if (this.dragging)
                {
                    this.viewModel.DragUpdate(p);
                }
                else if (this.mouseDown && (Math.Abs(diff.X) > 3 || Math.Abs(diff.Y) > 3))
                {
                    this.dragging = true;
                    this.viewModel.DragStart(this.start.ToImage(zoom));
                }
            }

            public void MouseUp(ImagePoint mousePosition)
            {
                if (this.dragging)
                {
                    this.viewModel.DragEnd();
                }
                else if (this.mouseDown)
                {
                    this.viewModel.Click(mousePosition);
                }

                this.dragging = false;
                this.mouseDown = false;
            }
        }
    }
}