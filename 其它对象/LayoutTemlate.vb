

Imports System.IO
''' <summary>
''' 打印模板定义
''' </summary>
Friend Class LayoutTemlate

#Region "字段"

    Friend Code As String = ""

    Friend FileName As String = ""

    Friend PathUrl As String = ""

    Friend FldMappingsList As New List(Of Tuple(Of String, String))

#End Region

#Region "动作"

    ''' <summary>
    ''' 打印指定的记录
    ''' </summary>
    ''' <param name="row"></param>
    Friend Sub Print(ByRef row As Data.DataRow, ByVal Printer As String, ByRef obj As SysSetting)

        Dim tmpFile As String = Path.Combine(PathUrl, FileName) '获得完整文件路径
        If Not File.Exists(tmpFile) Then Throw New Exception("错误：没有找到文件：" + tmpFile)

        Dim btApp As New BarTender.Application
        Dim btFormat As BarTender.Format

        ' 打开BarTender标签文件
        btFormat = btApp.Formats.Open(tmpFile, False, Printer)

        '逐个属性赋值 
        For Each mapping In FldMappingsList
            Try

                btFormat.SetNamedSubStringValue(mapping.Item2, row.Item(mapping.Item1).ToString)

            Catch ex As Exception
                Throw New Exception("打印字段：" + mapping.Item1 + " 时出现错误：" + ex.Message)
            End Try
        Next

        ' 执行打印操作，为作业命名以便于识别
        btFormat.Print("autoPrint_" + row.Item(obj.fld_data_id).ToString, 10000) ' 使用日期或其他标识符作为作业名称的一部分

        ' 关闭BarTender文档，不保存更改
        btFormat.Close(BarTender.BtSaveOptions.btDoNotSaveChanges)
        '关闭BarTender程序,不保存更改
        btApp.Quit(BarTender.BtSaveOptions.btDoNotSaveChanges)
        System.Runtime.InteropServices.Marshal.ReleaseComObject(btApp)
        '保存实际打印日期，更改状态
        Dim sb As New Text.StringBuilder("update [")
        sb.Append(obj.tb_Data).Append("] set [").Append(obj.fld_data_status).Append("] = 'Y',[")
        sb.Append(obj.fld_data_date).Append("] = convert(date,'").Append(Now.ToString("yyyyMMdd")).Append("',112),[")
        sb.Append(obj.fld_data_time).Append("] = ").Append(Now.ToString("HHmm")).Append(" where [")
        sb.Append(obj.fld_data_id).Append("] = ").Append(row.Item(obj.fld_data_id).ToString)

        Try
            DoSql(sb.ToString)
        Catch ex As Exception
            Throw New Exception("更新打印状态（ID = " + row.Item(obj.fld_data_id).ToString + "）时出现错误：" + ex.Message)
        End Try
    End Sub

#End Region


End Class
