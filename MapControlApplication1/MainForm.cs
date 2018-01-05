using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;//Ƕ�뻥��������:false //IWorkspaceFactory
using ESRI.ArcGIS.Geometry;//Ƕ�뻥��������:false //ISpatialReferenceFactory
using ESRI.ArcGIS.DataSourcesRaster;//Ƕ�뻥��������:false(->RasterWorkspaceFactoryClass) //IRasterWorkspace


namespace MapControlApplication1
{
    public sealed partial class MainForm : Form
    {
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;
        #endregion

        #region class constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //get the MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;

            //disable the Save menu (since there is no document yet)
            menuSaveDoc.Enabled = false;
        }

        #region Main Menu event handlers
        private void menuNewDoc_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuOpenDoc_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuSaveDoc_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (m_mapControl.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocumentClass();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("Map document is read only!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)m_mapControl.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            //execute SaveAs Document command
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuExitApp_Click(object sender, EventArgs e)
        {
            //exit the application
            Application.Exit();
        }
        #endregion

        //listen to MapReplaced evant in order to update the statusbar and the Save menu
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            //get the current document name from the MapControl
            m_mapDocumentName = m_mapControl.DocumentFilename;

            //if there is no MapDocument, diable the Save menu and clear the statusbar
            if (m_mapDocumentName == string.Empty)
            {
                menuSaveDoc.Enabled = false;
                statusBarXY.Text = string.Empty;
            }
            else
            {
                //enable the Save manu and write the doc name to the statusbar
                menuSaveDoc.Enabled = true;
                statusBarXY.Text = System.IO.Path.GetFileName(m_mapDocumentName);
            }
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }

        IWorkspace workspace;
        private void MI2_loadFromSDE_Click(object sender, EventArgs e)
        {
            //SDE�������ݿ��������
            IPropertySet  propertySet  =  new  PropertySet();
            //propertySet.SetProperty("DBCLIENT","postgresql");
            //propertySet.SetProperty("DB_CONNECTION_PROPERTIES","localhost");//���������߶˿�
            propertySet.SetProperty("SERVER","localhost"); 
            propertySet.SetProperty ("INSTANCE","sde:oracle11g:localhost/orcl"); 
            propertySet.SetProperty ("DATABASE" , "sde1363" ); 
            propertySet.SetProperty ("USER" , "sde" );
            propertySet.SetProperty("PASSWORD", "123456");
            propertySet.SetProperty("VERSION", "sde.DEFAULT");
            propertySet.SetProperty("AUTHENTICATION_MODE", "DBMS");

            //----- ���� SDE ���ݿ� -----//
            //ָ��SDE�����ռ�factory
            Type factoryType = Type. GetTypeFromProgID ("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            //����SDE���Ӳ������ô�SDE�����ռ�
            //set the private property -- workspace
            workspace = workspaceFactory.Open(propertySet, 0);

            //----- ��ʼ������դ��Ŀ¼���������� -----//
            //���դ��Ŀ¼�����������ѡ��
            //������դ��ͼ���е�դ��Ŀ¼
            cmb_loadRstCatalog.Items.Clear();
            cmb_loadRstCatalog.Text = "";
            cmb_loadRstCatalog.Items.Add("��դ��Ŀ¼(�����ռ�)");
            //������դ��ͼ���е�դ��Ŀ¼
            cmb_importRstCatalog.Items.Clear();
            cmb_loadRstCatalog.Text = "";
            cmb_importRstCatalog.Items.Add("��դ��Ŀ¼(�����ռ�)");

            //��ȡ���ݿ��е�դ��Ŀ¼��ȥ��SDEǰ׺
            IEnumDatasetName enumDatasetName  = workspace.get_DatasetNames(esriDatasetType. esriDTRasterCatalog); 
            IDatasetName datasetName  =  enumDatasetName.Next(); 
            while(datasetName != null)
            {
                cmb_loadRstCatalog.Items.Add(datasetName.Name.Substring(datasetName.Name.LastIndexOf(".")+1)) ; 
                //cmb_rstImgs.Items.Add(datasetName.Name.Substring(datasetName.Name.LastIndexOf(".")+1)); 
                datasetName = enumDatasetName.Next(); 
            }
            //����������Ĭ��ѡ��Ϊ��դ��Ŀ¼(�����ռ�)
            //������դ��ͼ���е�դ��Ŀ¼
            if (cmb_loadRstCatalog.Items.Count > 0)
            {
                cmb_loadRstCatalog.SelectedIndex = 0;
            }
            //������դ��ͼ���е�դ��Ŀ¼
            if (cmb_importRstCatalog.Items.Count > 0)
            {
                cmb_importRstCatalog.SelectedIndex = 0;
            }

        }

        //----- դ��Ŀ¼��դ��ͼ������������ʵ������ -----//
        private void cmb_rstCatalog_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rstCatalogName = cmb_loadRstCatalog.SelectedItem.ToString();
            IEnumDatasetName enumDatasetName;
            IDatasetName datasetName;
            if (cmb_loadRstCatalog.SelectedIndex == 0)
            {
                //���դ��ͼ�������������ѡ��
                cmb_loadRstDataset.Items.Clear();
                cmb_loadRstDataset.Text = "";
                //��ȡ��դ��Ŀ¼(�����ռ�)�е�դ��ͼ��
                enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterDataset);
                datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    cmb_loadRstDataset.Items.Add(datasetName.Name.Substring(datasetName.Name.LastIndexOf(".") + 1));
                    datasetName = enumDatasetName.Next();
                }
                //����������Ĭ��ѡ��Ϊ��դ��Ŀ¼(�����ռ䣩
                if (cmb_loadRstDataset.Items.Count > 0)
                    cmb_loadRstDataset.SelectedIndex = 0;
            }
            else
            {
                //�ӿ�ת��IRasterWorkspaceEx
                IRasterWorkspaceEx workspaceEx = (IRasterWorkspaceEx)workspace;
                //��ȡդ��Ŀ¼
                IRasterCatalog rasterCatalog = workspaceEx.OpenRasterCatalog(rstCatalogName); 
                //�ӿ�ת��IFeatureClass
                IFeatureClass featureClass = (IFeatureClass)rasterCatalog; 
                //�ӿ�ת��ITable
                ITable pTable = featureClass as ITable; 
                //ִ�в�ѯ��ȡָ��
                ICursor cursor = pTable.Search(null,  true)  as  ICursor; 
                IRow pRow = null;
                //����������ѡ��
                cmb_loadRstDataset.Items.Clear();
                cmb_loadRstDataset.Text = "";
                //ѭ��������ȡÿһ�м�¼
                while ((pRow = cursor.NextRow()) != null)
                {
                    int idxName = pRow.Fields.FindField ("NAME");
                    cmb_loadRstDataset.Items.Add(pRow.get_Value (idxName).ToString()); 
                }
                //����Ĭ��ѡ��
                if (cmb_loadRstDataset.Items.Count > 0)
                {
                    cmb_loadRstDataset.SelectedIndex = 0;
                }
                //�ͷ��ڴ�ռ�
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor) ; 
            }
        }

        //***** ���ݴ���->����դ��Ŀ¼ *****//
        //----- ����դ��Ŀ¼��Ĭ�ϲ���WGS84ͶӰ -----//
        private void btn_createRstCat_Click(object sender, EventArgs e)
        {
            if (txb_newRstCat.Text.Trim() == "")
                MessageBox.Show("������դ��Ŀ¼����!");
            else
            {
                string  rasCatalogName = txb_newRstCat.Text.Trim();
                IRasterWorkspaceEx rasterWorkspaceEx = workspace as IRasterWorkspaceEx;
                //����ռ�ο�����WGS84ͶӰ
                ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                ISpatialReference spatialReference = spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                spatialReference.SetDomain(-180, 180, -90, 90); 
                //�ж�դ��Ŀ¼�Ƿ����
                IEnumDatasetName enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterCatalog);
                IDatasetName datasetName = enumDatasetName.Next();
                bool isExit = false;
                //ѭ�������ж��Ƿ��Ѵ��ڸ�դ��Ŀ¼
                while (datasetName != null)
                {
                    if(datasetName.Name.Substring(datasetName. Name. LastIndexOf (".")+1) == rasCatalogName)
                    {
                        isExit = true;
                        MessageBox.Show("դ��Ŀ¼��������!");
                        txb_newRstCat.Focus();
                        return;
                    }
                    datasetName = enumDatasetName.Next(); 
                }

                //�������ڣ��򴴽��µ�դ��Ŀ¼
                if (isExit == false)
                {
                    //����դ��Ŀ¼�ֶμ�
                    IFields fields = CreateFields("RASTER","SHAPE",spatialReference,spatialReference); //CreateFields��������
                    rasterWorkspaceEx.CreateRasterCatalog(rasCatalogName,fields,"SHAPE","RASTER","DEFAULTS"); 
                    //���´�����դ��Ŀ¼��ӵ������б��У�������Ϊ��ǰդ��Ŀ¼
                    //cmb_LoadRstCatalog. Items.Add(rasCatalogName); 
                    //cmb_LoadRstCatalog.SelectedIndex  =  cmb_LoadRstCatalog.Items.Count - 1 ; 
                    cmb_loadRstCatalog.Items.Add(rasCatalogName); 
                    cmb_loadRstCatalog.SelectedIndex = cmb_loadRstCatalog.Items.Count - 1;
                    cmb_loadRstDataset.Items.Clear(); 
                    cmb_loadRstDataset.Text  = "";
                } 
                MessageBox.Show("դ��Ŀ¼�����ɹ�!");
            }
        }

        /// <summary>
        ///----- ����դ��Ŀ¼������ֶμ� -----// 
        /// </summary>
        /// <param name="rarasterFldName">Raster�ֶ�����</param>
        /// <param name="shapeFldName">Shape�ֶ�����</param>
        /// <param name="rarasterSF">Raster�ֶεĿռ�ο�</param>
        /// <param name="shapeSF">Shape�ֶεĿռ�ο�</param>
        private IFields CreateFields(string rasterFldName, string shapeFldName, ISpatialReference rasterSF, ISpatialReference shapeSF)
        {
            IFields fields  =  new  FieldsClass();
            IFieldsEdit fieldsEdit = fields as IFieldsEdit;
            IField field;
            IFieldEdit fieldEdit; 

            //���OID�ֶΣ�ע���ֶ�type
            field =  new  FieldClass(); 
            fieldEdit  =  field  as  IFieldEdit;
            fieldEdit.Name_2  = "ObjectID";
            fieldEdit.Type_2  =  esriFieldType.esriFieldTypeOID; 
            fieldsEdit.AddField(field);

            //���name�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "NAME";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField(field) ; 
            
            //���raster�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = rasterFldName;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeRaster;

            //IRasterDef�ӿڶ���դ���ֶ�
            IRasterDef rasterDef = new RasterDefClass();
            rasterDef.SpatialReference = rasterSF;
            ((IFieldEdit2)fieldEdit).RasterDef = rasterDef;
            fieldsEdit.AddField(field);

            //���shape�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = shapeFldName; 
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry; 
            
            //IGeometryDef��IGeometryDefEdit�ӿڶ���ͱ༭�����ֶ�
            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geometryDef as IGeometryDefEdit;
            geometryDefEdit.GeometryType_2 = esriGeometryType. esriGeometryPolygon;
            geometryDefEdit.SpatialReference_2 = shapeSF;
            ((IFieldEdit2)fieldEdit).GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field); 

            //���xml(Ԫ����)�ֶΣ�ע���ֶ�type
            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "METADATA";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeBlob;
            fieldsEdit.AddField(field);
            
            return fields; 
        }

        //***** ���ݴ���->����դ��ͼ�� *****//
        //----- ����ѡ����դ��Ŀ¼��դ��ͼ��,������Ӧ��դ��ͼ�� -----//
        private void btn_loadRstDataset_Click(object sender, EventArgs e)
        {
            if (cmb_loadRstCatalog.SelectedIndex == 0)
            {
                string rstDatasetName = cmb_loadRstDataset.SelectedItem.ToString();
                //�ӿ�ת��IRasterWorkspaceEx
                IRasterWorkspaceEx workspaceEx = (IRasterWorkspaceEx)workspace;
                //��ȡդ�����ݼ�
                IRasterDataset rasterDataset = workspaceEx.OpenRasterDataset(rstDatasetName);
                //����դ��Ŀ¼���դ��ͼ��
                IRasterLayer rasterLayer = new RasterLayerClass(); //IRasterLayer:����DataSourceRaster
                rasterLayer.CreateFromDataset(rasterDataset);
                ILayer layer = rasterLayer as ILayer;
                //��ͼ�������MapControl�У������ŵ���ǰͼ��
                axMapControl1.AddLayer(layer);
                axMapControl1.ActiveView.Extent = layer.AreaOfInterest;
                axMapControl1.ActiveView.Refresh();
                axTOCControl1.Update();
            }
            else
            {
                string rstCatalogName = cmb_loadRstCatalog.SelectedItem.ToString();
                String rstDatasetName = cmb_loadRstDataset.SelectedItem.ToString();
                //�ӿ�ת��IRasterWorkspaceEx
                IRasterWorkspaceEx workspaceEx = (IRasterWorkspaceEx) workspace; 
                //��ȡդ��Ŀ¼
                IRasterCatalog rasterCatalog = workspaceEx.OpenRasterCatalog(rstCatalogName);
                //�ӿ�ת��IFeatureClass
                IFeatureClass featureClass = (IFeatureClass)rasterCatalog;
                //�ӿ�ת��ITable
                ITable pTable = featureClass as ITable;
                //��ѯ����������QueryFilterClass
                IQueryFilter qf = new QueryFilterClass();
                qf.SubFields = "OBJECTID";
                qf.WhereClause = "NAME='" + rstDatasetName + "'";
                //ִ�в�ѯ��ȡָ��
                ICursor cursor = pTable.Search(qf, true) as ICursor;
                IRow pRow = null;
                int rstOID = 0;
                //�ж϶�ȡ��һ�м�¼
                if ((pRow = cursor.NextRow()) != null)
                {
                    int idxfld = pRow.Fields.FindField("OBJECTID"); 
                    rstOID = int. Parse (pRow.get_Value(idxfld).ToString());
                    //��ȡ��������դ��Ŀ¼��
                    IRasterCatalogItem rasterCatalogltem = (IRasterCatalogItem)featureClass.GetFeature(rstOID);
                    //����դ��Ŀ¼���դ��ͼ��
                    IRasterLayer rasterLayer = new RasterLayerClass();
                    rasterLayer.CreateFromDataset(rasterCatalogltem.RasterDataset);
                    ILayer layer = rasterLayer as ILayer;
                    //��ͼ�������MapControl�У������ŵ���ǰͼ��
                    axMapControl1.AddLayer(layer);
                    axMapControl1.ActiveView.Extent = layer.AreaOfInterest;
                    axMapControl1.ActiveView.Refresh();
                    axTOCControl1.Update(); 
                }
                //�ͷ��ڴ�ռ�
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor); 
            }
            iniCmbItems();
        }

        //***** ���ݴ���->����դ��ͼ�� *****//
        //----- ���դ��ͼ�񵯳��ļ�ѡ��Ի���ѡ��Ҫ�����դ��ͼ�� -----//
        private void txb_importRstDataset_Click(object sender, EventArgs e)
        {
            //���ļ�ѡ��Ի������öԻ�������
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imag file(*.img)|*.img|Tiff file(*.tif)|*.tif";
            openFileDialog.Title = "��Ӱ������";
            openFileDialog.Multiselect = false;
            string fileName = "";
            //����Ի��򼺳ɹ�ѡ���ļ������ļ�·����Ϣ��д���������
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
                txb_importRstDataset.Text = fileName;
            }
        }

        //----- ������룬��ѡ���դ��ͼ���빤���ռ��ָ����դ��Ŀ¼ -----//
        private void btn_importRstDataset_Click(object sender, EventArgs e)
        {
            //��ȡդ��ͼ���·�����ļ�����
            string fileName = txb_importRstDataset.Text; 
            FileInfo filelnfo = new FileInfo(fileName); 
            string filePath = filelnfo.DirectoryName; 
            string file = filelnfo. Name;
            string strOutName = file.Substring(0, file.LastIndexOf("."));
            //����·�����ļ����ֻ�ȡդ�����ݼ�
            if (cmb_importRstCatalog.SelectedIndex == 0)
            {
                //�ж��Ƿ�����������
                IWorkspace2 workspace2 = workspace as IWorkspace2;
                //������Ƽ�����
                if (workspace2.get_NameExists(esriDatasetType.esriDTRasterDataset, strOutName))
                {
                    DialogResult result;
                    result = MessageBox.Show(this, "��Ϊ " + strOutName + " ��դ���ļ������ݿ��м����ڣ�" + "��n�Ƿ񸲸ǣ�",
                        "��ͬ�ļ���", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    //���ѡ��ȷ��ɾ�����򸲸�ԭդ������
                    if (result == DialogResult.Yes)
                    {
                        IRasterWorkspaceEx rstWorkspaceEx = workspace as IRasterWorkspaceEx;
                        IDataset datasetDel = rstWorkspaceEx.OpenRasterDataset(strOutName) as IDataset;
                        //����IDataset�ӿڵ�Delete�ӿ�ʵ�ּ�����դ�����ݼ���ɾ��
                        datasetDel.Delete();
                        datasetDel = null;
                    }
                    else if (result == DialogResult.No)
                    {
                        MessageBox.Show("�����ռ伺����ͬ��դ�����ݼ��������ǲ��ܵ��룡");
                        return;
                    }
                }
                //����ѡ���դ��ͼ���·����դ�����ռ�
                IWorkspaceFactory rstWorkspaceFactoryImport = new RasterWorkspaceFactoryClass();
                IRasterWorkspace rstWorkspacelmport = (IRasterWorkspace)rstWorkspaceFactoryImport.OpenFromFile(filePath, 0);
                IRasterDataset rstDatasetlmport = null;
                //���ѡ���ļ���·���ǲ�����Ч��դ�����ռ�
                if (!(rstWorkspacelmport is IRasterWorkspace))
                {
                    MessageBox.Show("�ļ�·��������Ч��դ�����ռ䣡");
                    return;
                }
                //����ѡ���դ��ͼ������ֻ�ȡդ�����ݼ�
                rstDatasetlmport = rstWorkspacelmport.OpenRasterDataset(file);

                //��IRasterDataset�ӿڵ�CreateDefaultRaster���������հ׵�դ�����
                IRaster raster = rstDatasetlmport.CreateDefaultRaster();
                //IRasterProps �Ǻ�դ�����Զ����йصĽӿ�
                IRasterProps rasterProp = raster as IRasterProps;

                //IRasterStor��eDef�ӿں�դ�񴢴�����й�
                IRasterStorageDef storageDef = new RasterStorageDefClass();
                //ָ��ѹ������
                storageDef.CompressionType = esriRasterCompressionType.esriRasterCompressionLZ77;
                //����CellSize
                IPnt pnt = new PntClass();
                pnt.SetCoords(rasterProp.MeanCellSize().X, rasterProp.MeanCellSize().Y);
                storageDef.CellSize = pnt;
                //����դ�����ݼ���ԭ�㣬�������Ͻ�һ��λ�á�
                IPoint origin = new PointClass();
                origin.PutCoords(rasterProp.Extent.XMin, rasterProp.Extent.YMax);
                storageDef.Origin = origin;

                //�ӿ�ת��Ϊ��դ��洢�йص�ISaveAs2
                ISaveAs2 saveAs2 = (ISaveAs2)rstDatasetlmport;

                //�ӿ�ת��Ϊ��դ��洢���Զ����йص�IRasterStorageDef2
                IRasterStorageDef2 rasterStorageDef2 = (IRasterStorageDef2)storageDef;
                //ָ��ѹ����������Ƭ�߶ȺͿ��
                rasterStorageDef2.CompressionQuality = 100;
                rasterStorageDef2.Tiled = true;
                rasterStorageDef2.TileHeight = 128;
                rasterStorageDef2.TileWidth = 128;
                //������ ISaveAs2�ӿڵ�SaveAsRasterDataset����ʵ��դ�����ݼ��Ĵ洢
                //ָ���洢���֣������ռ䣬�洢��ʽ����ش洢����
                saveAs2.SaveAsRasterDataset(strOutName, workspace, "GRID", rasterStorageDef2);

                //��ʾ����ɹ�����Ϣ
                MessageBox.Show("����ɹ���");
            }
            else
            {
                string  rasterCatalogName  =  cmb_importRstCatalog.Text;
                //��դ�����ռ�
                IWorkspaceFactory pRasterWsFac = new  RasterWorkspaceFactoryClass();
                IWorkspace pWs = pRasterWsFac.OpenFromFile(filePath, 0);
                if (!(pWs is IRasterWorkspace))
                {
                    MessageBox.Show("�ļ�·��������Ч��դ�����ռ䣡");
                    return; 
                }
                IRasterWorkspace pRasterWs = pWs as IRasterWorkspace;
                //��ȡդ�����ݼ�
                IRasterDataset pRasterDs = pRasterWs.OpenRasterDataset(file);
                //����դ�����
                IRaster raster = pRasterDs.CreateDefaultRaster();
                IRasterProps rasterProp = raster as IRasterProps;
 
                //����դ�񴢴����
                IRasterStorageDef storageDef = new RasterStorageDefClass();
                storageDef.CompressionType = esriRasterCompressionType.esriRasterCompressionLZ77;
                //����CellSize
                IPnt pnt = new PntClass();
                pnt.SetCoords(rasterProp. MeanCellSize().X, rasterProp.MeanCellSize().Y);
                storageDef.CellSize = pnt;
                //����դ�����ݼ���ԭ�㣬�������Ͻ�һ��λ�á�
                IPoint origin = new PointClass();
                origin.PutCoords(rasterProp.Extent.XMin, rasterProp.Extent.YMax);
                storageDef.Origin =origin; 

                //��Raster Catalog �����դ��
                //�򿪶�Ӧ��Raster Catalog
                IRasterCatalog pRasterCatalog = ((IRasterWorkspaceEx)workspace).OpenRasterCatalog(rasterCatalogName);
                //����Ҫ�����RasterCatalogת����ΪFeatureClass
                IFeatureClass pFeatureClass = (IFeatureClass)pRasterCatalog;
                //���������е�������
                int nameIndex = pRasterCatalog.NameFieldIndex;
                //դ�����������е�������
                int rasterIndex = pRasterCatalog.RasterFieldIndex;
                IFeatureBuffer pBuffer = null;
                IFeatureCursor pFeatureCursor = pFeatureClass.Insert(false);
                //����IRasterValue�ӿڵĶ���:RasterBuffer�����rasterIndex��Ҫʹ��
                IRasterValue pRasterValue = new RasterValueClass();
                //����IRasterValue ��RasterDataset
                pRasterValue.RasterDataset = (IRasterDataset)pRasterDs;
                //�洢�����趨
                pRasterValue.RasterStorageDef = storageDef;
                pBuffer = pFeatureClass.CreateFeatureBuffer();
                //����RasterBuffer�����rasterIndex��nameIndex
                pBuffer.set_Value(rasterIndex, pRasterValue);
                pBuffer.set_Value(nameIndex, strOutName);
                //ͨ��cursorʵ��feature ��insert����
                pFeatureCursor.InsertFeature(pBuffer);
                pFeatureCursor.Flush();
                //�ͷ��ڴ���Դ
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pBuffer);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pRasterValue);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                //��ʾ�ɹ���Ϣ
                MessageBox.Show("����ɹ���");
            }
        }

        //***** TOCControl�ؼ���������¼� *****//
        //----- �����ʱ�ж����Ҽ��򵯳��˵� -----//
        ILayer TOCRightLayer;
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            try
            {
                //��ȡ��ǰ�����λ�õ������Ϣ
                esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap basicMap = null;
                ILayer layer = null;
                object unk = null;
                object data = null;

                //�����϶���Ľӿڶ�����Ϊ���ô��뺯���У���ȡ�������ֵ
                this.axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);

                //���������һ��ҵ��λ��Ϊͼ�㣬�򵯳��һ����ܿ�
                if (e.button == 2 && itemType == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    //����TOCѡ��ͼ��
                    this.TOCRightLayer = layer;
                    this.cms_TOCRightMenu.Show(axTOCControl1, e.x, e.y); 
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //----- �Ҽ��˵�->���ŵ���ǰͼ�� -----//
        private void tsmi_zoomToLayer_Click(object sender, EventArgs e)
        {
            try
            {
                //���ŵ���ǰͼ��
                axMapControl1.ActiveView.Extent = TOCRightLayer.AreaOfInterest;
                //ˢ��ҳ����ʾ
                axMapControl1.ActiveView.Refresh();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            { 
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //----- �Ҽ��˵�->ɾ����ǰͼ�� -----//
        private void tsmi_deleteLayer_Click(object sender, EventArgs e)
        {
            try
            {
                //ɾ����ǰͼ��
                axMapControl1.Map.DeleteLayer(TOCRightLayer);
                //ˢ�µ�ǰҳ��
                axMapControl1.ActiveView.Refresh();
                //���²�����Ϣͳ�Ƶ�ͼ��Ͳ���������ѡ������
                iniCmbItems();
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            { 
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //===== ͼ��Ͳ���ѡ��������ĳ�ʼ���������仯 =====//
        //-----  ��ʼ������ -----//
        //����ͼ��ʱ����ʼ����ͼ����tabҳ���ͼ��Ͳ����������ѡ������
        private void iniCmbItems()
        {
            try
            {
                //���������Ϣͳ��ͼ���������ѡ������
                cmb_statisticsLayer.Items.Clear();
                //����NDVIָ������ͼ���������ѡ������
                cmb_ndviLayer.Items.Clear();
                //���ֱ��ͼ����ͼ���������ѡ������
                cmb_drawHisLayer.Items.Clear();
                //��������λҶ���ǿ��ͼ���������ѡ������
                cmb_stretchLayer.Items.Clear();
                //����������α��ɫ��Ⱦ��ͼ���������ѡ������
                cmb_renderLayer.Items.Clear();
                //����ನ�μٲ�ɫ�ϳɵ�ͼ���������ѡ������
                cmb_rgbLayer.Items.Clear();
                ILayer layer = null;
                IMap map = axMapControl1.Map; 
                int count = map.LayerCount;
                if (count > 0)
                {
                    //������ͼ������ͼ�㣬��ȡͼ�����ּ���������
                    for (int i = 0; i < count; i++) 
                    {
                        layer = map.get_Layer(i);
                        //������Ϣͳ�Ƶ�ͼ��������
                        cmb_statisticsLayer.Items.Add(layer.Name);
                        //NDVIָ�������ͼ��������
                        cmb_ndviLayer.Items.Add(layer.Name);
                        //ֱ��ͼ���Ƶ�ͼ��������
                        cmb_drawHisLayer.Items.Add(layer.Name);
                        //�����λҶ���ǿ��ͼ��������
                        cmb_stretchLayer.Items.Add(layer.Name);
                        //������α��ɫ��Ⱦ��ͼ��������
                        cmb_renderLayer.Items.Add(layer.Name); 
                        //�ನ�μٲ�ɫ�ϳɵ�ͼ��������
                        cmb_rgbLayer. Items.Add(layer.Name);
                    }
                    //����������Ĭ��ѡ��Ϊ��һ��ͼ��
                    if (cmb_statisticsLayer.Items.Count > 0) 
                    {
                        cmb_statisticsLayer.SelectedIndex = 0; 
                    }
                    if (cmb_ndviLayer.Items.Count > 0)
                    {
                        cmb_ndviLayer.SelectedIndex = 0;
                    }
                    if (cmb_drawHisLayer.Items.Count > 0)
                    {
                        cmb_drawHisLayer.SelectedIndex = 0;
                    }
                    if (cmb_stretchLayer.Items.Count > 0)
                    {
                        cmb_stretchLayer.SelectedIndex = 0;
                    }
                    if (cmb_renderLayer.Items.Count > 0)
                    {
                        cmb_renderLayer.SelectedIndex = 0;
                    }
                    if (cmb_rgbLayer.Items.Count > 0)
                    {
                        cmb_rgbLayer.SelectedIndex = 0;
                    }

                    //���������Ϣͳ�Ʋ����������ѡ������
                    cmb_statisticsBand.Items.Clear();
                    //����ֱ��ͼ���ƵĲ����������ѡ������
                    cmb_drawHisBand.Items.Clear();
                    //��������λҶ���ǿ�Ĳ����������ѡ������
                    cmb_stretchBand.Items.Clear(); 
                    //���������α��ɫ��Ⱦ�Ĳ����������ѡ������
                    cmb_renderBand.Items.Clear(); 
                    //����ನ�μٲ�ɫ�ϳɵĲ����������ѡ������
                    cmb_RBand.Items.Clear();
                    cmb_GBand.Items.Clear();
                    cmb_BBand.Items.Clear(); 
                    //��ȡ��1��ͼ���դ�񲨶�
                    IRasterLayer rstLayer = map.get_Layer(0) as IRasterLayer;
                    IRaster2 raster2  = rstLayer.Raster as IRaster2; 
                    IRasterDataset rstDataset = raster2.RasterDataset;
                    IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;
                    //��������
                    int bandCount = rstLayer.BandCount; 
                    //������в��ε�ѡ��
                    cmb_statisticsBand.Items.Add("ȫ������");
                    //����ͼ������в��Σ���ȡ�������ּ���������
                    for (int i = 0; i < bandCount; i++)
                    {
                        int bandIdx = i + 1; //���ò������
                        //��Ӳ�����Ϣͳ�ƵĲ����������ѡ������
                        cmb_statisticsBand.Items.Add ( "����" + bandIdx);
                        //���ֱ��ͼ���ƵĲ����������ѡ������
                        cmb_drawHisBand.Items.Add ( "����" + bandIdx);
                        //��ӵ����λҶ���ǿ�Ĳ����������ѡ������
                        cmb_stretchBand.Items.Add ( "����" + bandIdx);
                        //��ӵ�����α��ɫ��Ⱦ�Ĳ����������ѡ������
                        cmb_renderBand.Items.Add ( "����" + bandIdx);
                        //��Ӷನ�μٲ�ɫ�ϳɵĲ����������ѡ������
                        cmb_RBand.Items.Add("����" + bandIdx);
                        cmb_GBand.Items.Add("����" + bandIdx);
                        cmb_BBand.Items.Add("����" + bandIdx);
                    }
                    //����������Ĭ��ѡ��
                    if (cmb_statisticsBand.Items.Count > 0)
                    {
                        cmb_statisticsBand.SelectedIndex = 0;
                    }
                    if (cmb_drawHisBand.Items.Count > 0)
                    {
                        cmb_drawHisBand.SelectedIndex = 0;
                    }
                    if (cmb_stretchBand.Items.Count > 0)
                    {
                        cmb_stretchBand.SelectedIndex = 0;
                    }
                    if (cmb_renderBand.Items.Count > 0)
                    {
                        cmb_renderBand.SelectedIndex = 0;
                    }
                    if (cmb_RBand.Items.Count > 0)
                    {
                        cmb_RBand.SelectedIndex = 0;
                    }
                    if (cmb_GBand.Items.Count > 0)
                    {
                        cmb_GBand.SelectedIndex = 0;
                    }
                    if (cmb_BBand.Items.Count > 0)
                    {
                        cmb_BBand.SelectedIndex = 0;
                    }
                }
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //-----  �����仯����  -----//
        //���������� �Լ�д ����������//
        //��ң��ͼ���������ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ��Ҳ�ᷢ���仯
        private void selectedIndexChangeFunction(ComboBox cmbLayer, ComboBox ctnbBand, string type)
        {
            try
            {
                //���������� �Լ�д ����������//
                //��ȡͼ���դ�񲨶�
                IMap map = axMapControl1.Map;
                int layerCount = map.LayerCount;
                IRasterLayer rstLayer = null;
                //����ͼ������ȡͼ��
                for (int i = 0; i < layerCount; i++)
                {
                    IRasterLayer tmpLayer = map.get_Layer(i) as IRasterLayer;
                    if (tmpLayer.Name == cmbLayer.Text)
                    {
                        rstLayer = tmpLayer;
                        break;
                    }
                }
                //��ȡ��ͼ���еĲ���
                if (rstLayer != null)
                {
                    //������������������е�ѡ��
                    ctnbBand.Items.Clear();
                    ctnbBand.Text = "";
                    //��ȡդ��ͼ����ײ��μ���
                    IRaster2 raster2 = rstLayer.Raster as IRaster2;
                    IRasterDataset rstDataset = raster2.RasterDataset;
                    IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;

                    //�������в���
                    int bandCount = rstLayer.BandCount;//��������
                    for (int j = 0; j < bandCount; j++)
                    {
                        //IRasterBand rb = rstBandColl.Item(j); //��ȡ����
                        int bandIdx = j + 1; //���ò������
                        //��Ӳ����������ѡ������
                        ctnbBand.Items.Add("����" + bandIdx);
                    }
                }
               
            }
            catch (System.Exception ex)//�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //-----  ͼ���������ѡ��仯�����¼� -----//
        //��������Ϣ��ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ����Ҳ�����仯
        private void cmb_statisticsLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_statisticsLayer, cmb_statisticsBand, "statistics");
            }
            catch (System. Exception ex) //�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //��ֱ��ͼ���Ƶ�ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ����Ҳ�����仯
        private void cmb_drawHisLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_drawHisLayer, cmb_statisticsBand, null);
            }
            catch (System.Exception ex) //�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //�������λҶ���ǿ��ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ����Ҳ�����仯
        private void cmb_stretchLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_stretchLayer, cmb_statisticsBand, null);
            }
            catch (System.Exception ex) //�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //��������α��ɫ��Ⱦ��ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ����Ҳ�����仯
        private void cmb_renderLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_renderLayer, cmb_statisticsBand, null);
            }
            catch (System.Exception ex) //�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //���ನ�μٲ�ɫ�ϳɵ�ͼ���������ѡ������仯������Ӧ�Ĳ����������ѡ����Ҳ�����仯
        private void cmb_rgbLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                selectedIndexChangeFunction(cmb_rgbLayer, cmb_RBand, null);
                selectedIndexChangeFunction(cmb_rgbLayer, cmb_GBand, null);
                selectedIndexChangeFunction(cmb_rgbLayer, cmb_BBand, null);
            }
            catch (System.Exception ex) //�쳣�������������Ϣ
            {
                MessageBox.Show(ex.Message, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //===== ������Ϣͳ�� =====//
        //���������� �Լ�д ����������//
        //->��8ҳ
        /// <summary>
        /// ������Ϣͳ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e��></param>
        private void btn_statistics_Click(object sender, EventArgs e)
        {
            try
            {
                IMap map = axMapControl1.Map;
                int layerCount = map.LayerCount;
                IRasterLayer rstLayer = null;
                //����ͼ������ȡͼ��
                for (int i = 0; i < layerCount; i++)
                {
                    IRasterLayer tmpLayer = map.get_Layer(i) as IRasterLayer;
                    if (tmpLayer.Name == cmb_statisticsLayer.Text)
                    {
                        rstLayer = tmpLayer;
                        break;
                    }
                }
                //��ȡդ��ͼ����ײ��μ���
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;
                
                //�������в���
                int bandCount = rstLayer.BandCount;//��������
                for (int i = 0; i < bandCount; i++)
                {
                    IRasterBand rasterBand = rstBandColl.Item(i); //��ȡ����
                }
            }
            finally
            {
            }
        }

        private void btn_blueAnalyse_Click(object sender, EventArgs e)
        {
            try
            {
                IMap map = axMapControl1.Map;
                int layerCount = map.LayerCount;
                IRasterLayer rstLayer = null;
                //����ͼ������ȡͼ��
                for (int i = 0; i < layerCount; i++)
                {
                    IRasterLayer tmpLayer = map.get_Layer(i) as IRasterLayer;
                    if (tmpLayer.Name == cmb_statisticsLayer.Text)
                    {
                        rstLayer = tmpLayer;
                        break;
                    }
                }
                //��ȡդ��ͼ����ײ��μ���
                IRaster2 raster2 = rstLayer.Raster as IRaster2;
                IRasterDataset rstDataset = raster2.RasterDataset;
                IRasterBandCollection rstBandColl = rstDataset as IRasterBandCollection;

                //�������в���
                int bandCount = rstLayer.BandCount;//��������
                for (int i = 0; i < bandCount; i++)
                {
                    IRasterBand rasterBand = rstBandColl.Item(i); //��ȡ����
                }

            }
            finally
            {
            }
        }
        


    }    
}