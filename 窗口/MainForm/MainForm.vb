
Imports System
Imports System.Xml
Imports System.Security.Cryptography
Imports System.Data.SqlClient
Imports System.Drawing.Printing
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Imports System.Linq
Imports System.Windows
Imports System.IO
Imports System.Reflection

Public Class MainForm

    Private _object As SysSetting   '����Դ

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            '===========================================================
            _object = New SysSetting()  '��ʼ������Դ
            _object.LoadFromFile()
            '===========================================================

            RefreshFormUI()

        Catch ex As Exception
            MsgBox("���������������ϵ����Ա��" & ex.Message)
            Me.Close()
            End
        End Try



    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If MessageBox.Show("��ȷ��Ҫ������е���־��", "ȷ��", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            txtLogo.Text = ""
        End If

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim oFSysSt As New FSysSt
        oFSysSt.SetObject(Me._object)
        AddHandler oFSysSt.AfterFormClosed, AddressOf RefreshFormUI
        oFSysSt.ShowDialog()

    End Sub


    Private Sub ShowMessageOnStatusStrip(txt As String)
        ' ��ʾ��Ϣ
        error_info.Text = txt
        error_info.BackColor = Color.Red
        error_info.ForeColor = Color.White

        AddLogo(txt)

        ' ����Timer
        Timer2.Enabled = True

    End Sub


    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        ' ����ı�
        error_info.Text = ""

        error_info.BackColor = Color.Transparent
        error_info.ForeColor = Color.Black

        ' ֹͣTimer
        Timer2.Enabled = False
    End Sub


    '��ȡ�����ļ���ˢ�½���
    Private Sub RefreshFormUI()

        Try
            '===========================================================
            ' ��ȡ���г����а�װ�Ĵ�ӡ��
            ComboBox1.Items.Clear()
            For Each printer As String In PrinterSettings.InstalledPrinters
                ComboBox1.Items.Add(printer)
            Next


            ' �������б����Ƿ����ָ����Ĭ�ϴ�ӡ��
            Dim itemIndex As Integer = ComboBox1.FindStringExact(_object.defaultPrinter)

            ' ������ڣ�������ComboBox����ʾ���ı�
            If itemIndex >= 0 Then
                ComboBox1.SelectedIndex = itemIndex
            Else
                ' ��������ڣ�����ʾ�հ�
                ComboBox1.Text = ""  ' �����ǰ��ʾ���ı�
                ComboBox1.SelectedIndex = -1 ' ȷ��û��ѡ���κ���
            End If
            '===========================================================


        Catch ex As Exception
            MsgBox("���������������ϵ����Ա��" & ex.Message)
            Me.Close()
            End
        End Try


        '===========================================================
        '�������ݿ� - ����ô�ӡ�������嵥
        Try
            Dim sqlstring = GetConnectString(_object) '���һ�������ַ���

            Try
                '��ô�ӡ����ʶ�嵥
                Dim sb As New Text.StringBuilder("select [")
                sb.Append(_object.fld_printerCode).Append("],[")
                sb.Append(_object.fld_printerName).Append("] from [")
                sb.Append(_object.tb_printers).Append("] ")

                Dim dt As DataTable = Rsco(sb.ToString, sqlstring)

                Dim options As New List(Of KeyValuePair(Of String, String))
                For i As Integer = 0 To dt.Rows.Count - 1
                    options.Add(New KeyValuePair(Of String, String)(dt.Rows.Item(i).Item(0).ToString, dt.Rows.Item(i).Item(0).ToString & " - " & dt.Rows.Item(i).Item(1).ToString))
                Next

                ' �󶨵�ComboBox
                With flagCombox
                    .DataSource = options
                    .DisplayMember = "Value"
                    .ValueMember = "Key"
                End With

                '����Ĭ��ֵ 
                If (_object.defaultPrinterFlag <> "") Then
                    If options.Any(Function(pair) pair.Key = _object.defaultPrinterFlag) Then
                        flagCombox.SelectedValue = _object.defaultPrinterFlag
                    End If
                End If

                ConnectionString = sqlstring '����������Ϣ

                flagCombox.Enabled = True '��ӡ����ʶ���������

            Catch ex As Exception
                ConnectionString = ""   '��������ַ���
                ShowMessageOnStatusStrip("����δ�ܻ�ô�ӡ��ʶ��Ϣ������ϵͳ���ã���ϸ��Ϣ��" & ex.Message)
            End Try



        Catch ex As Exception
            ShowMessageOnStatusStrip(�����󣺡� & ex.Message)
        End Try
        '===========================================================

        _object.SaveToFile() '��������

        '===========================================================
        '�������ӽ���������а�ť�ʹ�ӡ����ʶ�������Ƿ����
        Dim isConnected As Boolean = (ConnectionString <> "")
        flagCombox.Enabled = isConnected
        btnRun.Enabled = isConnected
        If isConnected Then
            StatusLabel.Text = "�����������ݿ⣺" & _object.DBName
            StatusLabel.ForeColor = Color.Blue

            AddLogo(StatusLabel.Text)
        Else
            StatusLabel.Text = "δ����"
            StatusLabel.ForeColor = Color.Black
        End If
        '===========================================================

    End Sub



    Friend Sub AddLogo(msg As String)
        ' ��ȡ��ǰ���ں�ʱ��
        Dim now As DateTime = DateTime.Now

        ' ת��Ϊ�ַ�������ʽΪ����-��-�� ʱ:��:�롱
        Dim dateTimeString As String = now.ToString("yyyy-MM-dd HH:mm:ss")
        Dim sb As New System.Text.StringBuilder(dateTimeString)
        sb.Append(vbTab).Append(msg).Append(vbCrLf).Append(txtLogo.Text)

        If txtLogo.InvokeRequired Then
            txtLogo.Invoke(New Action(Sub() txtLogo.Text = sb.ToString))
        Else
            txtLogo.Text = sb.ToString
        End If
    End Sub

    '������־
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Try
            ' ��ȡ��ǰִ�е�exe�ļ���Ŀ¼
            Dim exePath As String = Assembly.GetExecutingAssembly().Location
            Dim exeDirectory As String = Path.GetDirectoryName(exePath)

            ' ������־�ļ����ڵ���Ŀ¼���ļ���
            Dim logDirectory As String = Path.Combine(exeDirectory, "log")
            Dim FileName As String = Now.ToString("yyyy-MM-dd_HH_mm_ss")
            FileName = FileName & "_CHPrinter_Run_log.txt"

            FileName = Path.Combine(logDirectory, FileName)

            ' ��������ļ����ڵ�Ŀ¼�Ƿ���ڣ�����������򴴽�
            If Not Directory.Exists(logDirectory) Then
                Directory.CreateDirectory(logDirectory)
            End If

            File.WriteAllText(FileName, txtLogo.Text)

            If MessageBox.Show("��־�ļ��Ѵ�������" & FileName & vbCrLf & vbCrLf & "����Ҫ��յ�ǰ�����е���־��", "�����־", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                txtLogo.Text = ""
            End If

        Catch ex As Exception
            ShowMessageOnStatusStrip(ex.Message)
        End Try
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        '��ʼ����
        Try
            '======================================================
            '���ͱ���Ĭ��ֵ 
            Dim printer As String = ComboBox1.Text
            Dim printerFlag As String = flagCombox.SelectedValue.ToString

            If (printer = "") Then Throw New Exception("����δѡ�񱾵ش�ӡ�����޷�������")
            If (printerFlag = "") Then Throw New Exception("����δѡ���ӡ����ʶ���޷�������")

            _object.defaultPrinterFlag = printerFlag
            _object.defaultPrinter = printer
            '======================================================




            '======================================================
            '����SQL��� - ֻ���δ��ӡ�ģ����Ҵ�ӡ��ʶ���Լ���ͬ�ļ�¼
            Dim sb As New Text.StringBuilder
            sb.Append(" where [")
            sb.Append(_object.fld_data_status).Append("] = 'N' and [")
            sb.Append(_object.fld_data_printerFlag).Append("] = N'").Append(printerFlag).Append("'")

            query_condition = sb.ToString
            query_str = "select [" + _object.fld_data_id + "] from dbo.[" + _object.tb_Data + "] " + query_condition
            '======================================================


            'MsgBox(query_str)
            '======================================================
            ' ��ʼ�������ݿ�֪ͨ
            AddLogo("��ʼִ�� - �����������������")

            DoJob() '������һ��

            StartListening(ConnectionString)
            btnRun.Enabled = False '��ť������
            '======================================================

        Catch ex As Exception
            ShowMessageOnStatusStrip(ex.Message)
        End Try
    End Sub


#Region ����ѯ֪ͨ��ش���"

    Private query_str As String = ""     '��ش�ӡ��Ϣ���SQL��ѯ����
    Private query_condition As String = ""   '���/��ȡ��ӡ��Ϣ�������

    Private Sub StartListening(connectionString As String)
        ' ����SqlDependency��SQL Server��ͨ��
        SqlDependency.Start(connectionString)
        Try
            Using connection As New SqlConnection(connectionString)
                connection.Open()

                Using command As SqlCommand = connection.CreateCommand()
                    command.CommandText = query_str

                    ' ����SqlDependency����OnChange�¼�
                    Dim dependency As New SqlDependency(command)
                    AddHandler dependency.OnChange, AddressOf OnDependencyChange

                    command.ExecuteReader().Dispose()

                    AddLogo("�ɹ����������������")
                    sts_run.Text = "����������"

                End Using
            End Using
        Catch ex As Exception
            ShowMessageOnStatusStrip("��������(StartListening): " & ex.Message)
            SqlDependency.Stop(connectionString)
            sts_run.Text = "δ����"
        End Try
    End Sub



    Private Sub OnDependencyChange(sender As Object, e As SqlNotificationEventArgs)

        '=====================================================================
        ' �Ƴ��¼���������Ա��������������Ի�����������
        Dim dependency As SqlDependency = CType(sender, SqlDependency)
        RemoveHandler dependency.OnChange, AddressOf OnDependencyChange
        '=====================================================================

        StartListening(ConnectionString)

        DoJob()



    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        End
    End Sub

    Private Sub DoJob()
        Try
            '=====================================================================
            Dim printer As String = CType(Me.Invoke(New Func(Of String)(Function() ComboBox1.Text)), String)
            Dim printerFlag As String = CType(Me.Invoke(New Func(Of String)(Function() Convert.ToString(flagCombox.SelectedValue))), String)

            ' �����ﴦ��֪ͨ�¼�
            AddLogo("�����Ҫ��ӡ������������������,��ӡ����ʶ��" + printerFlag + " ��ӡ����" + printer)

            '�Ȱѱ�������ݶ��뵽����
            Dim query_data As String = "select * from dbo.[" + _object.tb_Data + "] " + query_condition
            Dim dt_data As System.Data.DataTable = Rsco(query_data)


            '���Զ��뵽���ص����ݽ��д���
            Dim ct As Integer = dt_data.Rows.Count
            AddLogo("������Ҫ��ӡ�����ݣ����ƣ�" + ct.ToString + " ����¼")

            For i As Integer = 0 To ct - 1

                Dim flag As String = (i + 1).ToString + "/" + ct.ToString

                AddLogo(flag + "���ڻ�ô�ӡģ�����ϸ����...")
                '==================================================================
                Dim tmplateCode As String = dt_data.Rows.Item(i).Item(_object.fld_data_template)
                Dim layout As LayoutTemlate = oLayoutManager.GetLayout(tmplateCode, _object)

                AddLogo(flag + "���ڴ�ӡ����" + printer)
                layout.Print(dt_data.Rows.Item(i), printer, _object)
                '==================================================================

            Next


            '=====================================================================
        Catch ex As Exception
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Sub() ShowMessageOnStatusStrip("�����ӡ����ʱ��������: " & ex.Message)))
            Else
                ShowMessageOnStatusStrip("�����ӡ����ʱ��������: " & ex.Message)
            End If


        End Try
    End Sub

    Private Sub Label26_Click(sender As Object, e As EventArgs) Handles Label26.Click

    End Sub

#End Region
End Class
