<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:param name="Subject" />
	<xsl:param name="DateTime" />
	<xsl:param name="SourceFolder" />
	<xsl:param name="SourceFileName" />
	<xsl:param name="OutputFileName" />
	<xsl:param name="DestinationFolder" />
	<xsl:param name="SupplierName" />
	<xsl:param name="ERPID" />
	<xsl:param name="Environment" />
	<xsl:template match="/NewDataSet">
			<html>
			<head>
				<title><xsl:value-of select="Subject" /></title>
				<style>
					body {
						font-family: Arial Verdana Helvetica sans-serif;
						font-size: 10pt;
					}
					p {
						font-family: Arial Verdana Helvetica sans-serif;
						font-size: 10pt;
					}
					th {
						font-family: Arial Verdana Helvetica sans-serif;
						font-size: 10pt;
					}
					td {
						font-family: Arial Verdana Helvetica sans-serif;
						font-size: 10pt;
					}
					.Header
					{
					BACKGROUND-COLOR: rgb(65,130,195);
					FONT-FAMILY: Arial, Verdana, Tahoma;
						FONT-SIZE: 9pt;
					COLOR: #FFFFFF;
					FONT-WEIGHT:BOLD;
					}
					.Details
					{
					FONT-FAMILY: Arial, Verdana, Tahoma;
					FONT-SIZE: 9pt;
					}
					.Title
					{
					FONT-FAMILY: Arial, Verdana, Tahoma;
					FONT-SIZE: 10pt;
					FONT-WEIGHT:BOLD;
					}
					.Back
					{
					BACKGROUND-COLOR: #E8EEF7;
					
					}
				</style>
			</head>
			<body>
								
								
				<table align="center" border="0" width="100%">
				
				<tr>
					<td colspan="2" align="right" class="Title">
					<img src="http://www.gsk.com/common/img/logo-gsk.gif" /><BR/>
					<b><xsl:value-of select="Subject" /></b></td>
				</tr>
				</table>


				<table align="center" width="98%" border="0" cellpadding="3" cellspacing="1" class="Back">
				<tr>
				<th align="left" class="header" width="30%" >Biztalk Process Date</th>
				<th align="left" width="70%" bgcolor="#FFFFFF"><xsl:value-of select="DateTime" /></th>
				</tr>
				<tr>
				<th align="left" class="header" width="30%" >Source Folder</th>
				<th align="left" width="70%" bgcolor="#F9F8F8"><xsl:value-of select="SourceFolder" /></th>
				</tr>
				<tr>
				<th align="left" class="header" width="30%" >SourceFileName</th>
				<th align="left" width="70%" bgcolor="#FFFFFF"><xsl:value-of select="SourceFileName" /></th>
				</tr>
				<tr>
				<th align="left" class="header" >Output File Name</th>
				<th align="left"  bgcolor="#F9F8F8"><xsl:value-of select="OutputFileName" /></th>
				</tr>
				<tr>
				<th align="left" class="header" >Destination Folder</th>
				<th align="left"  bgcolor="#FFFFFF"><xsl:value-of select="DestinationFolder" /></th>
				</tr>
				<tr>
				<th align="left" class="header" >Supplier</th>
				<th align="left"  bgcolor="#F9F8F8"><xsl:value-of select="SupplierName" /></th>
				</tr>
				<tr>
				<th align="left" class="header" >SDS Environment</th>
				<th align="left"  bgcolor="#FFFFFF"><xsl:value-of select="ERPID" /></th>
				</tr>
				<tr>
				<th align="left" class="header" >Solution Environment</th>
				<th align="left"  bgcolor="#F9F8F8"><xsl:value-of select="Environment" /></th>
				</tr>
				</table><BR/><BR/>
				<p class="details">You may reply to this e-mail for any concerns.</p><br/>
			</body>
		</html>
		</xsl:template>
</xsl:stylesheet>