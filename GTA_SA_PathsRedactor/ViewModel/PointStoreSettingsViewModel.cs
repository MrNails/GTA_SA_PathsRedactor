using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services;
using Microsoft.Win32;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PointStoreSettingsViewModel : Core.Entity
    {
        private readonly ProxyController _proxyController;
        
        private readonly ICommand _loadAssemblyCommand;

        private TreeNodeWithItem? _currentLoader;
        private TreeNodeWithItem? _currentSaver;

        public PointStoreSettingsViewModel(ProxyController proxyController)
        {
            Loaders = new ObservableCollection<TreeNodeWithItem>();
            Savers = new ObservableCollection<TreeNodeWithItem>();
            _proxyController = proxyController;

            _loadAssemblyCommand = new RelayCommand(LoadAssemblyCommandHandler);

            LoadDefaultInfo();
            InitializeAssembliesInfos();
        }

        public TreeNodeWithItem? CurrentLoader
        {
            get => _currentLoader;
            set
            {
                _currentLoader = value;
                OnPropertyChanged();
            }
        }
        public TreeNodeWithItem? CurrentSaver
        {
            get => _currentSaver;
            set
            {
                _currentSaver = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TreeNodeWithItem> Loaders { get; }
        public ObservableCollection<TreeNodeWithItem> Savers { get; }

        public ICommand LoadAssemblyCommand => _loadAssemblyCommand;

        private TreeNodeWithItem? GetExistAssemblyNode(AssemblyInfo assemblyInfo, IEnumerable<TreeNodeWithItem> treeNodeWithItems)
        {
            foreach (var node in treeNodeWithItems)
            {
                if (node.Element is not AssemblyInfo info)
                    continue;

                if (info == assemblyInfo)
                    return node;
            }

            return null;
        }

        private void InitializeAssembliesInfos()
        {
            foreach (var assembly in _proxyController.Assemblies)
            {
                LoadAssemblyInfo(assembly.GetAssemblyInfo(), assembly.Location);
            }
        }

        private void LoadAssemblyInfo(AssemblyInfo assemblyInfo, string assemblyLocation)
        {
            var jsonFileInfo = assemblyLocation.Substring(0, assemblyLocation.LastIndexOf('.')) + ".json";

            if (File.Exists(jsonFileInfo))
            {
                var derivedLoaders = _proxyController.GetDerivedTypesFromAssembly(assemblyInfo.FullName, typeof(Core.IPointLoader))
                                                    .Select(type => type.FullName);
                var derivedSavers = _proxyController.GetDerivedTypesFromAssembly(assemblyInfo.FullName, typeof(Core.IPointSaver))
                                                   .Select(type => type.FullName);

                ParseCustomInfo(File.ReadAllText(jsonFileInfo),
                                (loaders, savers) =>
                                {
                                    var joinedLoaders = loaders.Join(derivedLoaders,
                                                                     outer => outer.Name,
                                                                     inner => inner,
                                                                     (outer, inner) => outer);
                                    var joinedSavers = savers.Join(derivedSavers,
                                                                   outer => outer.Name,
                                                                   inner => inner,
                                                                   (outer, inner) => outer);

                                    var resultLoaders = derivedLoaders.Except(joinedLoaders.Select(jLoader => jLoader.Name))
                                                                      .Select(dLoader => new ItemInfo(dLoader, string.Empty, string.Empty))
                                                                      .Union(joinedLoaders);
                                    var resultSavers = derivedSavers.Except(joinedSavers.Select(jSaver => jSaver.Name))
                                                                    .Select(dSaver => new ItemInfo(dSaver, string.Empty, string.Empty))
                                                                    .Union(joinedSavers);

                                    var loadersNode = new TreeNodeWithItem(assemblyInfo);
                                    var saversNode = new TreeNodeWithItem(assemblyInfo);

                                    loadersNode.DisplayMember = saversNode.DisplayMember = "Title";
                                    saversNode.ValueMember = loadersNode.ValueMember = "FullName";

                                    FillNode(loadersNode, resultLoaders, "Name");
                                    FillNode(saversNode, resultSavers, "Name");

                                    if (resultLoaders.Any())
                                        Loaders.Add(loadersNode);

                                    if (resultSavers.Any())
                                        Savers.Add(saversNode);
                                });
            }
            else
            {
                var loaderNode = CreateNodeFromAssembly(assemblyInfo, typeof(Core.IPointLoader));
                var saverNode = CreateNodeFromAssembly(assemblyInfo, typeof(Core.IPointSaver));

                saverNode.DisplayMember = loaderNode.DisplayMember = "Title";
                saverNode.ValueMember = loaderNode.ValueMember = "FullName";

                Loaders.Add(loaderNode);
                Savers.Add(saverNode);
            }
        }

        private void LoadDefaultInfo()
        {
            var fileSource = new string(Encoding.UTF8.GetChars(AppResources.DefaultInfo));

            ParseCustomInfo(fileSource, (loaders, savers) => 
                            {
                                var currentAssembly = this.GetType().Assembly.GetAssemblyInfo();
                                var defaultInfo = new AssemblyInfo("Default", currentAssembly.FullName, currentAssembly.Version,
                                                                   currentAssembly.Author, currentAssembly.Company, currentAssembly.Product,
                                                                   "Default saver and loader for using program.");

                                var loadersNode = new TreeNodeWithItem(defaultInfo);
                                var saversNode = new TreeNodeWithItem(defaultInfo);

                                loadersNode.DisplayMember = saversNode.DisplayMember = "Title";
                                saversNode.ValueMember = loadersNode.ValueMember = "FullName";

                                foreach (var loader in loaders)
                                {
                                    var loaderNode = new TreeNodeWithItem(loader, "Name");
                                    loaderNode.ValueMember = "Name";

                                    loadersNode.Nodes.Add(loaderNode);
                                }
                                foreach (var saver in savers)
                                {
                                    var saverNode = new TreeNodeWithItem(saver, "Name");
                                    saverNode.ValueMember = "Name";

                                    saversNode.Nodes.Add(saverNode);
                                }

                                CurrentLoader = (TreeNodeWithItem)loadersNode.Nodes[0];
                                CurrentSaver = (TreeNodeWithItem)saversNode.Nodes[0];

                                Loaders.Add(loadersNode);
                                Savers.Add(saversNode);
                            });
        }
        
        private void FillNode<T>(TreeNodeWithItem node, IEnumerable<T> infos, string infoDisplayMember)
        {
            foreach (var info in infos)
            {
                var _node = new TreeNodeWithItem(info);
                _node.DisplayMember = infoDisplayMember;
                _node.ValueMember = infoDisplayMember;

                node.Nodes.Add(_node);
            }
        }

        private TreeNodeWithItem CreateNodeFromAssembly(AssemblyInfo assemblyInfo, Type baseType)
        {
            var derivedTypes = _proxyController.GetDerivedTypesFromAssembly(assemblyInfo.FullName, baseType);
            var topmostNode = new TreeNodeWithItem(assemblyInfo);

            foreach (var type in derivedTypes)
            {
                var node = new TreeNodeWithItem(new ItemInfo(type.FullName, string.Empty, string.Empty));

                topmostNode.Nodes.Add(node);
                node.DisplayMember = "Name";
                node.ValueMember = "Name";
            }

            return topmostNode;
        }

        private void LoadAssemblyCommandHandler()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dynamic-link library files (*.dll)|*.dll";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                var assemblyInfo = _proxyController.AddAssembly(fileName).GetAssemblyInfo();

                var existSaverNode = GetExistAssemblyNode(assemblyInfo, Savers);
                var existLoaderNode = GetExistAssemblyNode(assemblyInfo, Loaders);

                if (existSaverNode != null || existLoaderNode != null)
                {
                    CurrentSaver = (TreeNodeWithItem)existSaverNode?.Nodes.FirstOrDefault();
                    CurrentLoader = (TreeNodeWithItem)existLoaderNode?.Nodes.FirstOrDefault();
                    return;
                }

                LoadAssemblyInfo(assemblyInfo, fileName);
            }
        }

        private void ParseCustomInfo(string jsonString, Action<IEnumerable<ItemInfo?>, IEnumerable<ItemInfo?>> endAction)
        {
            var result = JsonNode.Parse(jsonString);
            
            if (result is JsonObject jObj)
            {
                Func<JsonNode, ItemInfo> transformToken = token =>
                    {
                        var name = token["Name"]?.GetValue<string>();
                        var description = token["Description"]?.GetValue<string>();
                        var purpose = token["Purpose"]?.GetValue<string>();
            
                        return new ItemInfo(name, purpose, description);
                    };
            
                var loaders = (jObj["Loaders"] as JsonArray).Select(transformToken);
                var savers = (jObj["Savers"] as JsonArray)
                                 .Select(transformToken);
            
                endAction(loaders, savers);
            }
            else
            {
                throw new InvalidOperationException("Cannot parse info file.");
            }
        }
    }
}
