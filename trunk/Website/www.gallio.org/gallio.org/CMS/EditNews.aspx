﻿<%@ Page Language="C#" MasterPageFile="~/CMS/CMS.master" AutoEventWireup="true" CodeFile="EditNews.aspx.cs" Inherits="CMS_EditNews" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <h2>
        Edit News</h2>
    <p>
        <asp:DetailsView ID="DetailsView1" runat="server" AllowPaging="True" AutoGenerateDeleteButton="True"
            AutoGenerateEditButton="True" AutoGenerateRows="False" DataKeyNames="ID"
            DataSourceID="SqlDataSource1" Height="183px"
            Width="350px">
            <Fields>
                <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"
                    SortExpression="ID" />
                <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                <asp:TemplateField HeaderText="Text" SortExpression="Text">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox1" runat="server" Rows="5" Text='<%# Bind("Text") %>' TextMode="MultiLine"
                            Width="360px" Height="152px"></asp:TextBox>
                    </EditItemTemplate>
                    <InsertItemTemplate>
                        <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Text") %>'></asp:TextBox>
                    </InsertItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%# Bind("Text") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Created" HeaderText="Created" SortExpression="Created" />
                <asp:BoundField DataField="Author" HeaderText="Author" SortExpression="Author" />
            </Fields>
        </asp:DetailsView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:gallioDBConnectionString1 %>"
            SelectCommand="SELECT * FROM [NewsItems]" ConflictDetection="CompareAllValues" DeleteCommand="DELETE FROM [NewsItems] WHERE [ID] = @original_ID AND [Title] = @original_Title AND [Text] = @original_Text AND [Created] = @original_Created AND [Author] = @original_Author" InsertCommand="INSERT INTO [NewsItems] ([Title], [Text], [Created], [Author]) VALUES (@Title, @Text, @Created, @Author)" OldValuesParameterFormatString="original_{0}" UpdateCommand="UPDATE [NewsItems] SET [Title] = @Title, [Text] = @Text, [Created] = @Created, [Author] = @Author WHERE [ID] = @original_ID AND [Title] = @original_Title AND [Text] = @original_Text AND [Created] = @original_Created AND [Author] = @original_Author">
            <DeleteParameters>
                <asp:Parameter Name="original_ID" Type="Int32" />
                <asp:Parameter Name="original_Title" Type="String" />
                <asp:Parameter Name="original_Text" Type="String" />
                <asp:Parameter Name="original_Created" Type="DateTime" />
                <asp:Parameter Name="original_Author" Type="String" />
            </DeleteParameters>
            <UpdateParameters>
                <asp:Parameter Name="Title" Type="String" />
                <asp:Parameter Name="Text" Type="String" />
                <asp:Parameter Name="Created" Type="DateTime" />
                <asp:Parameter Name="Author" Type="String" />
                <asp:Parameter Name="original_ID" Type="Int32" />
                <asp:Parameter Name="original_Title" Type="String" />
                <asp:Parameter Name="original_Text" Type="String" />
                <asp:Parameter Name="original_Created" Type="DateTime" />
                <asp:Parameter Name="original_Author" Type="String" />
            </UpdateParameters>
            <InsertParameters>
                <asp:Parameter Name="Title" Type="String" />
                <asp:Parameter Name="Text" Type="String" />
                <asp:Parameter Name="Created" Type="DateTime" />
                <asp:Parameter Name="Author" Type="String" />
            </InsertParameters>
        </asp:SqlDataSource>
        &nbsp;</p>
</asp:Content>

