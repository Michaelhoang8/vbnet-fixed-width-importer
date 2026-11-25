Imports System
Imports System.IO
Imports System.Globalization
Imports System.Collections.Generic
Imports System.Text.Json

Public Class ImportedRecord
    Public Property Id As String = ""
    Public Property Posted As String = ""
    Public Property Cents As Long
End Class

Public Class ImportResult
    Public Property Accepted As New List(Of ImportedRecord)
    Public Property Rejected As New List(Of String)
End Class

Module Program
    Private Function Digits(value As String) As Boolean
        For Each character As Char In value
            If character < "0"c OrElse character > "9"c Then Return False
        Next
        Return value.Length > 0
    End Function

    Public Function Parse(lines As IEnumerable(Of String)) As ImportResult
        Dim result As New ImportResult()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim row As Integer = 0
        For Each line As String In lines
            row += 1
            Dim reason As String = ""
            Dim value As New ImportedRecord()
            If line.Length <> 21 Then
                reason = "expected 21 ASCII characters"
            Else
                value.Id = line.Substring(0, 4)
                Dim dayText As String = line.Substring(4, 8)
                Dim sign As Char = line(12)
                Dim amountText As String = line.Substring(13, 8)
                Dim day As DateTime
                If Not Digits(value.Id) Then
                    reason = "invalid identifier"
                ElseIf Not Digits(dayText) OrElse Not DateTime.TryParseExact(dayText, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, day) Then
                    reason = "invalid calendar date"
                ElseIf sign <> "+"c AndAlso sign <> "-"c Then
                    reason = "invalid sign"
                ElseIf Not Digits(amountText) Then
                    reason = "invalid amount"
                ElseIf seen.Contains(value.Id) Then
                    reason = "duplicate accepted identifier"
                Else
                    value.Posted = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    value.Cents = Long.Parse(amountText, CultureInfo.InvariantCulture) * If(sign = "-"c, -1L, 1L)
                End If
            End If
            If reason.Length > 0 Then
                result.Rejected.Add($"line {row}: {reason}")
            Else
                seen.Add(value.Id)
                result.Accepted.Add(value)
            End If
        Next
        Return result
    End Function

    Private Sub Check(condition As Boolean, label As String)
        If Not condition Then Throw New Exception(label)
    End Sub

    Private Sub Tests()
        Dim good As String = "000120240229+00001250"
        Dim result As ImportResult = Parse({good, "000220250228-00000350", good, "000320250229+00000001", "bad"})
        Check(result.Accepted.Count = 2 AndAlso result.Rejected.Count = 3, "accepted/rejected counts")
        Check(result.Accepted(0).Posted = "2024-02-29" AndAlso result.Accepted(0).Cents = 1250, "leap day")
        Check(result.Accepted(1).Cents = -350, "signed minor units")
        Check(result.Rejected(0).StartsWith("line 3:"), "source row provenance")
        Check(Parse({"000120240229x00001250"}).Rejected.Count = 1, "sign")
        Check(Parse({"000120240229+0000A250"}).Rejected.Count = 1, "numeric amount")
        Check(Parse({"000120240229+00000000"}).Accepted(0).Cents = 0, "zero")
        Check(Parse({"000120240229+99999999"}).Accepted(0).Cents = 99999999, "maximum")
        Check(Parse({"000120250229+00000001", good}).Accepted.Count = 1, "rejected IDs do not reserve identifier")
        Check(Parse(Array.Empty(Of String)()).Accepted.Count = 0, "empty input")
        Check(Parse({good & " "}).Rejected.Count = 1, "trailing characters")
        Console.WriteLine("11 fixed-width checks passed")
    End Sub

    Function Main(args As String()) As Integer
        Try
            If args.Length = 1 AndAlso args(0) = "--test" Then
                Tests()
                Return 0
            End If
            If args.Length <> 1 OrElse args(0) = "--help" Then
                Console.WriteLine("Usage: dotnet run -- records.txt | --test")
                Return If(args.Length = 1, 0, 2)
            End If
            Console.WriteLine(JsonSerializer.Serialize(Parse(File.ReadLines(args(0))), New JsonSerializerOptions With {.WriteIndented = True}))
            Return 0
        Catch ex As Exception
            Console.Error.WriteLine(ex.Message)
            Return 1
        End Try
    End Function
End Module
