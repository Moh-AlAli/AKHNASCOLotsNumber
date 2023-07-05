Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Security.Cryptography
Imports System.IO
Imports System.Text
Imports acc = ACCPAC.Advantage

Friend Class crviewer
    Private rdoc As New ReportDocument
    Private conrpt As New ConnectionInfo()
    Dim server As String = ""
    Dim uid As String = ""
    Dim pass As String = ""
    Dim fdate As String = ""
    Dim tdate As String = ""
    Private ccompid As String
    Private ccompname As String
    Private cfdate As String
    Private ctdate As String
    Private cfrminvno As String
    Private ctoinvno As String
    Private cfrmlot As String
    Private ctolot As String
    Private crbinv As Boolean
    Private crblot As Boolean

    Friend Property ObjectHandle As String
    Friend Function createdes(ByVal key As String) As TripleDES
        Dim md5 As MD5 = New MD5CryptoServiceProvider()
        Dim des As TripleDES = New TripleDESCryptoServiceProvider()
        des.Key = md5.ComputeHash(Encoding.Unicode.GetBytes(key))
        des.IV = New Byte(des.BlockSize \ 8 - 1) {}
        Return des
    End Function

    Friend Function Decryption(ByVal cyphertext As String, ByVal key As String) As String
        Dim b As Byte() = Convert.FromBase64String(cyphertext)
        Dim des As TripleDES = createdes(key)
        Dim ct As ICryptoTransform = des.CreateDecryptor()
        Dim output As Byte() = ct.TransformFinalBlock(b, 0, b.Length)
        Return Encoding.Unicode.GetString(output)
    End Function
    Friend Function Readconnectionstring() As String

        Dim secretkey As String = "Fhghqwjehqwlegtoit123mnk12%&4#"
        Dim path As String = ("txtcon\akhcon.txt")
        Dim sr As New StreamReader(path)

        server = sr.ReadLine()
        Dim db As String = sr.ReadLine()
        uid = sr.ReadLine()
        pass = sr.ReadLine()


        server = Decryption(server, secretkey)
        uid = Decryption(uid, secretkey)
        pass = Decryption(pass, secretkey)

        Dim cons As String = ""

        Return cons
    End Function
    Public Sub New(ByVal _objectHandle As String, ByVal _sess As acc.Session, ByVal fdate As String, ByVal tdate As String, ByVal frminvno As String, ByVal toinvno As String, ByVal fromlot As String, ByVal tolot As String, ByVal rbinv As Boolean, ByVal rblot As Boolean)
        InitializeComponent()
        ObjectHandle = _objectHandle
        ccompid = _sess.CompanyID
        ccompname = _sess.CompanyName
        cfdate = fdate
        ctdate = tdate
        cfrminvno = frminvno
        ctoinvno = toinvno
        cfrmlot = cfrmlot
        ctolot = tolot
        crbinv = rbinv
        crblot = rblot


    End Sub

    Public Sub New(ByVal _objectHandle As String)
        InitializeComponent()
        ObjectHandle = _objectHandle
    End Sub

    Private Sub CrystalReportViewer1_Load(sender As Object, e As EventArgs) Handles cryviewer.Load
        Try
            If crbinv = True Then
                rdoc.Load("reports\Lotsalesbyinvoices.rpt")
            ElseIf crblot = True Then
                rdoc.Load("reports\invoicesLotnumber.rpt")
            End If


            Dim tabs As Tables = rdoc.Database.Tables





            fdate = cfdate
            tdate = ctdate


            Readconnectionstring()
            For Each TAB As CrystalDecisions.CrystalReports.Engine.Table In tabs

                Dim tablog As TableLogOnInfo = TAB.LogOnInfo
                conrpt.ServerName = server
                conrpt.DatabaseName = ccompid
                conrpt.UserID = uid
                conrpt.Password = pass
                tablog.ConnectionInfo = conrpt
                TAB.ApplyLogOnInfo(tablog)
            Next

            rdoc.SetParameterValue("aFrmDate", "'" & fdate & "'")
            rdoc.SetParameterValue("aTodate", "'" & tdate & "'")
            rdoc.SetParameterValue("bFrminvno", "'" & cfrminvno & "'")
            rdoc.SetParameterValue("bToinvno", "'" & ctoinvno & "'")
            rdoc.SetParameterValue("cFrmlot", "'" & cfrmlot & "'")
            rdoc.SetParameterValue("cTolot", "'" & ctolot & "'")
            rdoc.SetParameterValue("CONAME", ccompname)


            cryviewer.ReportSource = rdoc


        Catch ex As Exception
            MessageBox.Show(ex.Message)
            If rdoc Is Nothing Then
                rdoc.Close()
                rdoc.Dispose()
            End If
        End Try
    End Sub



End Class