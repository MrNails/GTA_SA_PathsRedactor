using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GTA_SA_PathsRedactor.Services;
using Microsoft.Win32;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PathVM
    {
        private PathEditor m_pathEditor;

        private RelayCommand m_addPointCommand;
        private RelayCommand m_insertPointCommand;
        private RelayCommand m_removePointCommand;
        private RelayCommand m_removeSelectedPointsCommand;

        private RelayCommand m_savePath;
        private RelayCommand m_loadPath;

        public PathVM() : this(new PathEditor("New path"))
        { }
        public PathVM(string pathName) : this(new PathEditor(pathName))
        { }
        public PathVM(PathEditor pathEditor)
        {
            if (pathEditor == null)
            {
                throw new ArgumentNullException("pathEditor");
            }

            m_pathEditor = pathEditor;

            //m_addPointCommand = new RelayCommand(obj =>
            //{
            //    var inputElem = obj as IInputElement;

            //    m_pathEditor.AddPoint(new Models.GTA_SA_Point(Mouse.GetPosition(inputElem), 0, false));
            //});
            m_removePointCommand = new RelayCommand(obj =>
            {
                m_pathEditor.RemovePoint(obj as Models.VisualObject);
            }, obj => obj is Models.VisualObject);
            m_removeSelectedPointsCommand = new RelayCommand(obj =>
            {
                m_pathEditor.RemoveSelectedPoint();
            }, obj => m_pathEditor.SelectedDots.Count != 0);

            m_loadPath = new RelayCommand(async obj =>
            {
                PointLoader pointLoader = null;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "exe files (*.ext) |*.exe";

                if (openFileDialog.ShowDialog() == true)
                {
                    pointLoader = new PointLoader(openFileDialog.FileName);

                    m_pathEditor.Clear();

                    m_pathEditor.AddRangePoint(await pointLoader.LoadPointsAsync());
                }
            });
        }

        public PathEditor PathEditor => m_pathEditor;

        public RelayCommand AddPointCommand => m_addPointCommand;
        public RelayCommand InsertPointCommand => m_insertPointCommand;
        public RelayCommand RemovePointCommand => m_removePointCommand;
        public RelayCommand RemoveSelectedPointsCommand => m_removeSelectedPointsCommand;

        public RelayCommand SavePath => m_savePath;
        public RelayCommand LoadPath => m_loadPath;
    }
}
