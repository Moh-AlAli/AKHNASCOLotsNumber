﻿
Imports acc = ACCPAC.Advantage
Imports System.Runtime.InteropServices


Friend Class srclotnum


    Public compid As String = ""
    Public frmcust As String
    Public Tocust As String
    Public fdate As String
    Public tdate As String

    Friend Property ERPSession As acc.Session
    Friend Property Company As ERPCompany
    Friend Property SessionDate As String
    Friend Property ObjectHandle As String

    Private _oldVendNumb As String = ""
    <DllImport("a4wroto.dll", EntryPoint:="rotoSetObjectWindow", CharSet:=CharSet.Ansi)>
    Private Shared Sub rotoSetObjectWindow(
        <MarshalAs(UnmanagedType.I8)> ByVal objectHandle As Long,
        <MarshalAs(UnmanagedType.I8)> ByVal hWnd As Long)
    End Sub

    Public Sub New(ByVal ses As acc.Session, ByVal comp As ERPCompany, ByVal sesDate As String)
        InitializeComponent()
        'ObjectHandle = ""
        ERPSession = ses
        Company = comp
        compid = comp.ID

        SessionDate = sesDate

    End Sub

    Public Sub New(ByVal _objectHandle As String)
        InitializeComponent()
        ObjectHandle = _objectHandle
    End Sub
    Public Sub New()
        InitializeComponent()

    End Sub
    Private Sub fndEditBoxValidate(ByVal sender As Object, ByVal e As EventArgs)
        If CMDClose.Focused Then Return
        Dim txb As TextBox = CType(sender, TextBox)
        If String.IsNullOrEmpty(txb.Text) Then Return
        Dim msg As String = ""
        Dim s As String() = New String() {}

        Select Case txb.Name.Trim()
            Case "txtfrmcus"

                If _oldVendNumb.Trim() <> txb.Text.Trim() Then
                    msg = getValidationData("select ID=IDCUST,NAM=NAMECUST,SW=SWACTV from ARCUS where IDCUST='" & txb.Text & "'", s)

                    If msg <> "" Then
                        MessageBox.Show(Me, msg, "Cutomer Statement", MessageBoxButtons.OK, MessageBoxIcon.[Error])
                        Return
                    End If

                    If s.Length = 0 Then
                        MessageBox.Show(Me, "Customer """ & txb.Text & """ does not exists.", "Invoice Daily", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        txb.Focus()
                        txb.SelectAll()
                        Return
                    End If

                    If s(2).Trim() = "0" Then
                        MessageBox.Show(Me, "Customer """ & txb.Text & """ is not active.", "Invoice Daily", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        txb.Focus()
                        txb.SelectAll()
                        Return
                    End If


                End If
                Txtfrminvno.Text = s(0)
            Case "Txttocus"

                If _oldVendNumb.Trim() <> txb.Text.Trim() Then
                    msg = getValidationData("select ID=IDCUST,NAM=NAMECUST,SW=SWACTV from ARCUS where IDCUST='" & txb.Text & "'", s)

                    If msg <> "" Then
                        MessageBox.Show(Me, msg, "Invoice Daily", MessageBoxButtons.OK, MessageBoxIcon.[Error])
                        Return
                    End If

                    If s.Length = 0 Then
                        MessageBox.Show(Me, "Customer """ & txb.Text & """ does not exists.", "Invoice Daily", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        txb.Focus()
                        txb.SelectAll()
                        Return
                    End If

                    If s(2).Trim() = "0" Then
                        MessageBox.Show(Me, "Customer """ & txb.Text & """ is not active.", "Invoice Daily", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        txb.Focus()
                        txb.SelectAll()
                        Return
                    End If


                End If


                Txttoinvno.Text = s(0)
                ' End If
        End Select
    End Sub
    Private Function getValidationData(ByVal sql As String, <Out> ByRef data As String()) As String
        data = New String(2) {}
        Dim hasRecs As Boolean = False

        Try
            Dim lnk As acc.DBLink = ERPSession.OpenDBLink(acc.DBLinkType.Company, acc.DBLinkFlags.[ReadOnly])
            Dim opQry As acc.View = lnk.OpenView("CS0120")
            opQry.Cancel()
            opQry.Browse(sql, True)
            opQry.InternalSet(256)

            While opQry.Fetch(False)
                hasRecs = True
                data(0) = opQry.Fields.FieldByName("ID").Value.ToString().Trim()
                data(1) = opQry.Fields.FieldByName("NAM").Value.ToString().Trim()
                data(2) = opQry.Fields.FieldByName("SW").Value.ToString().Trim()

            End While

            opQry.Dispose()
            lnk.Dispose()
            If Not hasRecs Then data = New String() {}
            Return ""
        Catch ex As Exception
            Dim erstr As String = ""
            Dim erlst As List(Of String) = New List(Of String)()
            Util.FillErrors(ex, ERPSession, erlst)

            For Each s As String In erlst
                erstr += s & vbCrLf
            Next

            Dim ms As String = "Sage 300 ERP Error: " & erstr
            Return ms
        End Try
    End Function
    Private Sub SessionFromERP(ByVal frmHwnd As IntPtr)
        Dim lhWnd As Long = Nothing

        Try
            If ERPSession Is Nothing Then ERPSession = New acc.Session()
            If ERPSession.IsOpened Then ERPSession.Dispose()
            ERPSession.Init(ObjectHandle, "AS", "AS0001", "69A")

            If Not Long.TryParse(ObjectHandle, lhWnd) Then
                MessageBox.Show("Invalid Sage 300 ERP object handle.", "Invoice Daily Utility", MessageBoxButtons.OK, MessageBoxIcon.[Error])
                ERPSession.Dispose()
                Return
            End If

            rotoSetObjectWindow(lhWnd, frmHwnd.ToInt64())
            Company = New ERPCompany(ERPSession.CompanyName, ERPSession.CompanyID)
            SessionDate = ERPSession.SessionDate.ToString()
        Catch ex As Exception
            Dim erstr As String = ""
            Dim erlst As List(Of String) = New List(Of String)()
            Util.FillErrors(ex, ERPSession, erlst)

            For Each s As String In erlst
                erstr += s & vbCrLf
            Next

            Dim ms As String = "Sage 300 ERP Error: " & erstr
            ERPSession.Dispose()
            MessageBox.Show(ms, "Invoice Daily", MessageBoxButtons.OK, MessageBoxIcon.[Error])
            Return
        End Try
    End Sub



    Private Sub custstatement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try

            If Not ObjectHandle Is Nothing Then
                SessionFromERP(Handle)
            End If
            Txttoinvno.Text = "zzzzzzzzzzzzzzzzzzzzzz"


            rblot.Checked = True

        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Close()
        End Try
    End Sub

    Private Sub CMDCloseClick(sender As Object, e As EventArgs) Handles CMDClose.Click
        Close()
    End Sub

    Private Sub Butprint_Click(sender As Object, e As EventArgs) Handles Butprint.Click
        Try

            Dim fmonthnew As String = 0
            If DateTimePicker1.Value.Month.ToString.Length < 2 Then
                fmonthnew = "0" & DateTimePicker1.Value.Month
            Else
                fmonthnew = DateTimePicker1.Value.Month
            End If
            Dim tmonthnew As String = 0
            If DateTimePicker2.Value.Month.ToString.Length < 2 Then
                tmonthnew = "0" & DateTimePicker2.Value.Month
            Else
                tmonthnew = DateTimePicker2.Value.Month
            End If

            Dim fdaynew As String = 0
            If DateTimePicker1.Value.Day.ToString.Length < 2 Then
                fdaynew = "0" & DateTimePicker1.Value.Day
            Else
                fdaynew = DateTimePicker1.Value.Day
            End If
            Dim tdaynew As String = 0
            If DateTimePicker2.Value.Day.ToString.Length < 2 Then
                tdaynew = "0" & DateTimePicker2.Value.Day
            Else
                tdaynew = DateTimePicker2.Value.Day
            End If

            fdate = DateTimePicker1.Value.Year & fmonthnew & fdaynew

            tdate = DateTimePicker2.Value.Year & tmonthnew & tdaynew

            Dim toinvno As String = ""
            If Txttoinvno.Text = Nothing Then
                toinvno = "zzzzzzzzzzzzzzzzzzzzzz"
            Else
                toinvno = Trim(Txttoinvno.Text)
            End If
            Dim tolot As String = ""
            If txttolot.Text = Nothing Then
                tolot = "zzzzzzzzzzzzzzzzzzzzzz"
            Else
                tolot = Trim(txttolot.Text)
            End If

            If Trim(Txtfrminvno.Text) <= Trim(Txttoinvno.Text) Then
                If fdate <= tdate Then
                    If Trim(txtfrmlot.Text) <= Trim(txttolot.Text) Then
                        Dim f As crviewer = New crviewer(ObjectHandle, ERPSession, fdate, tdate, Trim(Txtfrminvno.Text), toinvno, Trim(txtfrmlot.Text), tolot, rbinv.Checked, rblot.Checked)
                        f.Show()
                    Else
                        MessageBox.Show("From Lot No greater than To Lot No")
                    End If
                Else
                    MessageBox.Show("From Date  greater than To Date")
                End If
            Else
                MessageBox.Show("From Invoice No greater than To Invoice No")
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub Bffind_Click(sender As Object, e As EventArgs) Handles Bffind.Click
        Dim f As FromFinder = New FromFinder("OEINVH", "Invoices", New String() {"INVNUMBER"}, ERPSession, "", "")

        Dim r As DialogResult = f.ShowDialog(Me)
        If r = DialogResult.OK Then
            Txtfrminvno.Text = f.Result.ToArray()(0)
            Txttoinvno.Text = f.Result.ToArray()(0)
            fndEditBoxValidate(Txtfrminvno, EventArgs.Empty)
        End If
    End Sub

    Private Sub Btfind_Click(sender As Object, e As EventArgs) Handles Btfind.Click
        Dim f As FromFinder = New FromFinder("OEINVH", "Invoices", New String() {"INVNUMBER"}, ERPSession, "", "")
        Dim r As DialogResult = f.ShowDialog(Me)
        If r = DialogResult.OK Then
            Txttoinvno.Text = f.Result.ToArray()(0)
            fndEditBoxValidate(Txttoinvno, EventArgs.Empty)
        End If
    End Sub

    Private Sub pbflot_Click(sender As Object, e As EventArgs) Handles pbflot.Click
        Dim f As FromFinder = New FromFinder("ICXLHIS", "Lots", New String() {"LOTNUM"}, ERPSession, "", "")
        Dim r As DialogResult = f.ShowDialog(Me)
        If r = DialogResult.OK Then
            txtfrmlot.Text = f.Result.ToArray()(0)
            fndEditBoxValidate(txtfrmlot, EventArgs.Empty)
        End If
    End Sub

    Private Sub pbtlot_Click(sender As Object, e As EventArgs) Handles pbtlot.Click
        Dim f As FromFinder = New FromFinder("ICXLHIS", "Lots", New String() {"LOTNUM"}, ERPSession, "", "")
        Dim r As DialogResult = f.ShowDialog(Me)
        If r = DialogResult.OK Then
            txttolot.Text = f.Result.ToArray()(0)
            fndEditBoxValidate(txttolot, EventArgs.Empty)
        End If
    End Sub
End Class
