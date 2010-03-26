<%@ Page Title="List" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Collections.Generic.List<PhotoShare.PhotoInfo>>" %>
<%@ Import Namespace="photo_share_site" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	List
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Photos</h2>

	<%
		var cur_page  = (ViewData["cur_page"] as int?) ?? 1;
		var page_size = (ViewData["page_size"] as int?) ?? Model.Count;
	%>
	
	<p>
	<%= cur_page > 1 ? Html.ActionLink("Previous Page", "List", new { id = cur_page - 1 }) : MvcHtmlString.Empty %>
	&nbsp;&nbsp;&nbsp;&nbsp;
	<%= Model.Count >= page_size ? Html.ActionLink("Next Page", "List", new { id = cur_page + 1 }) : MvcHtmlString.Empty %>
	</p>
	
	<table>
	<% foreach( var g in Model.GroupIn(4) ) { %>
		<tr>
		<% foreach( var p in g ) { %>
			<td>
				<%=p.Filename%><br />
				<a href="<%=Url.Action("Image", new { id = p.PhotoId })%>">
					<img src="<%=Url.Action("Thumb", new { id = p.PhotoId })%>" alt="<%=p.Title%>" />
				</a>
			</td>
		<% } %>
		</tr>
	<% } %>
	</table>
	
	<p>
	<%= cur_page > 1 ? Html.ActionLink("Previous Page", "List", new { id = cur_page - 1 }) : MvcHtmlString.Empty %>
	&nbsp;&nbsp;&nbsp;&nbsp;
	<%= Model.Count >= page_size ? Html.ActionLink("Next Page", "List", new { id = cur_page + 1 }) : MvcHtmlString.Empty %>
	</p>
</asp:Content>
