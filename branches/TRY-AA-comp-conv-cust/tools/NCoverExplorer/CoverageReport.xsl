<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt">
    <!-- Created for NCoverExplorer by Grant Drake (see http://www.kiwidude.com/blog/) -->
    <!-- Modified by Atif Aziz (see http://www.raboof.com/) -->
    <xsl:output method="html"  doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN"/>
	<xsl:template match="/">
		<xsl:apply-templates select="//coverageReport" />					
	</xsl:template>
	<xsl:template match="coverageReport">
		<html>
			<head>
				<xsl:comment>Generated by NCoverExplorer (see http://www.kiwidude.com/blog/)</xsl:comment>
				<title><xsl:value-of select="./project/@name" /></title>
	<style>
        body 						{ font: small arial, helvetica; color:#000000; background-color: #fff; }
        h1                          { font-size: large; }
        h2                          { font-size: medium; }
        th                          { text-align: left; }
        .coverageReportTable		{ font-size: small; margin-top: 1em; border-collapse: collapse; }
        table.coverageReportTable td, th { padding: 0.3em; }
        .reportHeader 				{ padding: 5px 8px 5px 8px; font-weight: bold; font-size: medium; border: 1px solid; margin: 0px;	}
        .titleText					{ font-weight: bold; font-size: medium; white-space: nowrap; padding: 0px; margin: 1px; }
        .subtitleText 				{ font-size: small; font-weight: normal; padding: 0px; margin: 1px; white-space: nowrap; }
        .projectStatistics			{ font-size: small; border-left: #649cc0 1px solid; white-space: nowrap;	width: 40%;	}
        .heading					{ font-weight: bold; }
        .mainTableHeaderLeft 		{ border: #dcdcdc 1px solid; font-weight: bold;	padding-left: 5px; }
        .mainTableHeader 			{ border-bottom: 1px solid; border-top: 1px solid; border-right: 1px solid;	text-align: center;	}
        .mainTableGraphHeader 		{ border-bottom: 1px solid; border-top: 1px solid; border-right: 1px solid;	text-align: left; font-weight: bold; }
        .mainTableCellItem 			{ background: #ffffff; border-left: #dcdcdc 1px solid; border-right: #dcdcdc 1px solid; padding-left: 10px; padding-right: 10px; }
        .mainTableCellData 			{ background: #ffffff; border-right: #dcdcdc 1px solid;	text-align: center;	white-space: nowrap; }
        .mainTableCellPercent 		{ background: #ffffff; white-space: nowrap; text-align: right; padding-left: 10px; }
        .mainTableCellGraph 		{ background: #ffffff; border-right: #dcdcdc 1px solid; padding-right: 5px; }
        .mainTableCellBottom		{ border-bottom: #dcdcdc 1px solid;	}
        .childTableHeader 			{ border-top: 1px solid; border-bottom: 1px solid; border-left: 1px solid; border-right: 1px solid;	font-weight: bold; padding-left: 10px; }
        .childTableCellIndentedItem { background: #ffffff; border-left: #dcdcdc 1px solid; border-right: #dcdcdc 1px solid; padding-left: 20px; padding-right: 10px; }
        .exclusionTableCellItem 	{ background: #ffffff; border-left: #dcdcdc 1px solid; border-right: #dcdcdc 1px solid; padding-left: 10px; padding-right: 10px; }
        .projectTable				{ background: #a9d9f7; border-color: #649cc0; }
        .primaryTable				{ background: #d7eefd; border-color: #a4dafc; }
        .secondaryTable 			{ background: #f9e9b7; border-color: #f6d376; }
        .secondaryChildTable 		{ background: #fff6df; border-color: #f5e1b1; }
        .exclusionTable				{ background: #fadada; border-color: #f37f7f; }
        .graphBarNotVisited			{ font-size: 2px; border:#9c9c9c 1px solid; background:#df0000; }
        .graphBarSatisfactory		{ font-size: 2px; border:#9c9c9c 1px solid;	background:#f4f24e; }
        .graphBarVisited			{ background: #00df00; font-size: 2px; border-left:#9c9c9c 1px solid; border-top:#9c9c9c 1px solid; border-bottom:#9c9c9c 1px solid; }
        #Stats                      { border: none; border-collapse: collapse; }
        #Stats th, td               { border-bottom: 1px solid #dcdcdc; }
        #Stats th                   { font-weight: normal; text-align: left; }
        #Stats td                   { padding-left: 2em; text-align: right; }
        hr                          { display: none; }
    </style>
	</head>
	<body>
        <h1>
            <xsl:value-of select="./project/@name" /> Coverage Report
        </h1>
        <p>
            Report generated on <xsl:value-of select="./@date" /> at <xsl:value-of select="./@time" />.
            Acceptable coverage is <xsl:value-of select="concat(format-number(./project/@acceptable,'#0.0'), '%')" />.
        </p>
        <h2>Statistics</h2>
        <table id="Stats">
            <tr><th>Files</th>
            <td><xsl:value-of select="format-number(./project/@files, '###,##0')" /></td></tr>
            <tr><th>Classes</th>
            <td><xsl:value-of select="format-number(./project/@classes, '###,##0')" /></td></tr>
            <tr><th>Members</th>
            <td><xsl:value-of select="format-number(./project/@members, '###,##0')" /></td></tr>
            <tr><th>Non-Comment Lines</th>
            <td><xsl:value-of select="format-number(./project/@nonCommentLines, '###,##0')" /></td></tr>
            <tr><th>Total Points</th>
            <td><xsl:value-of select="format-number(./project/@sequencePoints, '###,##0')" /></td></tr>
            <tr><th>Unvisited</th>
            <td><xsl:value-of select="format-number(./project/@unvisitedPoints, '###,##0')" /></td></tr>                
        </table>
        <h2>Coverage</h2>
        <table class="coverageReportTable">
            <tbody>

                <xsl:variable name="reportType" select="//coverageReport/@reportTitle" />
                <xsl:variable name="threshold" select="//coverageReport/project/@acceptable" />

                <xsl:call-template name="projectSummary">
                    <xsl:with-param name="threshold" select="$threshold" />
                </xsl:call-template>


                <xsl:if test="$reportType = 'Module Summary' or $reportType = 'Module Namespace Summary'">
                    <xsl:call-template name="moduleSummary">
                        <xsl:with-param name="threshold" select="$threshold" />
                    </xsl:call-template>
                </xsl:if>

                <xsl:if test="$reportType = 'Module Namespace Summary'">
                    <xsl:call-template name="moduleNamespaceSummary">
                        <xsl:with-param name="threshold" select="$threshold" />
                    </xsl:call-template>
                </xsl:if>

                <xsl:if test="$reportType = 'Namespace Summary'">
                    <xsl:call-template name="namespaceSummary">
                        <xsl:with-param name="threshold" select="$threshold" />
                    </xsl:call-template>
                </xsl:if>

                <xsl:if test="count(./exclusions) != 0">
                    <xsl:call-template name="exclusionsSummary" />
                </xsl:if>

            </tbody>
        </table>
        <xsl:call-template name="footer" />
    </body>
</html>
	</xsl:template>

    <!-- Report Header -->
	<xsl:template name="header">
				<tr>
					<td class="projectTable reportHeader" colspan="4">
						<table width="100%">
							<tbody>
								<tr>
									<td valign="top">
										<h1 class="titleText">
                                            <xsl:value-of select="./project/@name" /> Coverage Report</h1>
										<table cellpadding="1" class="subtitleText">
											<tbody>
												<tr>
													<td class="heading">Acceptable coverage is <xsl:value-of select="concat(format-number(./project/@acceptable,'#0.0'), ' %')" />
                                                </td>
													<td></td>
												</tr>
												<tr>
													<td class="heading">Report generated on <xsl:value-of select="./@date" />&#160;at&#160;<xsl:value-of select="./@time" />
                                                </td>
													<td></td>
												</tr>
											</tbody>
										</table>
									</td>
									<td class="projectStatistics" align="right" valign="top">
										<table cellpadding="1">
											<tbody>
												<tr>
													<td rowspan="3" valign="top" nowrap="true">Project Statistics:</td>
													<td align="right">Files:</td>
													<td align="right"><xsl:value-of select="./project/@files" /></td>
													<td rowspan="3">&#160;</td>
													<td align="right">NCLOC:</td>
													<td align="right"><xsl:value-of select="./project/@nonCommentLines" /></td>
												</tr>
												<tr>
													<td align="right">Classes:</td>
													<td align="right"><xsl:value-of select="./project/@classes" /></td>
													<td align="right">Total Pts:</td>
													<td align="right"><xsl:value-of select="./project/@sequencePoints" /></td>
												</tr>
												<tr>
													<td align="right">Members:</td>
													<td align="right"><xsl:value-of select="./project/@members" /></td>
													<td align="right">Unvisited:</td>
													<td align="right"><xsl:value-of select="./project/@unvisitedPoints" /></td>
												</tr>
											</tbody>
										</table>
									</td>
								</tr>
							</tbody>
						</table>
					</td>
				</tr>
	</xsl:template>
	
	<!-- Project Summary -->
	<xsl:template name="projectSummary">
		<xsl:param name="threshold" />
				<tr>
					<th class="projectTable mainTableHeaderLeft">Project</th>
					<th class="projectTable mainTableHeader">Unvisited</th>
					<th class="projectTable mainTableGraphHeader" colspan="2">Coverage</th>
				</tr>
			<xsl:call-template name="coverageDetail">
				<xsl:with-param name="name" select="./project/@name" />
				<xsl:with-param name="unvisitedPoints" select="./project/@unvisitedPoints" />
				<xsl:with-param name="sequencePoints" select="./project/@sequencePoints" />
				<xsl:with-param name="coverage" select="./project/@coverage" />
				<xsl:with-param name="threshold" select="$threshold" />
			</xsl:call-template>
	</xsl:template>
		
	<!-- Modules Summary -->
	<xsl:template name="moduleSummary">
		<xsl:param name="threshold" />
				<tr>
					<th class="primaryTable mainTableHeaderLeft">Modules</th>
					<th class="primaryTable mainTableHeader">Unvisited</th>
					<th class="primaryTable mainTableGraphHeader" colspan="2">Coverage</th>
				</tr>				
		<xsl:for-each select="//coverageReport/modules/module">
			<xsl:call-template name="coverageDetail">
				<xsl:with-param name="name" select="./@name" />
				<xsl:with-param name="unvisitedPoints" select="./@unvisitedPoints" />
				<xsl:with-param name="sequencePoints" select="./@sequencePoints" />
				<xsl:with-param name="coverage" select="./@coverage" />
				<xsl:with-param name="threshold" select="$threshold" />
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>
		
	<!-- Namespaces per Module Summary -->
	<xsl:template name="moduleNamespaceSummary">
		<xsl:param name="threshold" />
		<xsl:for-each select="//coverageReport/modules/module">
				<tr>
					<th class="secondaryTable mainTableHeaderLeft">Module</th>
					<th class="secondaryTable mainTableHeader">Unvisited</th>
					<th class="secondaryTable mainTableGraphHeader" colspan="2">Coverage</th>
				</tr>				
			<xsl:call-template name="coverageDetailSecondary">
				<xsl:with-param name="name" select="./@name" />
				<xsl:with-param name="unvisitedPoints" select="./@unvisitedPoints" />
				<xsl:with-param name="sequencePoints" select="./@sequencePoints" />
				<xsl:with-param name="coverage" select="./@coverage" />
				<xsl:with-param name="threshold" select="$threshold" />
			</xsl:call-template>
				<tr>
					<td class="secondaryChildTable childTableHeader" colspan="4">Namespaces</td>
				</tr>				
			<xsl:for-each select="./namespace">
				<xsl:call-template name="coverageIndentedDetail">
					<xsl:with-param name="name" select="./@name" />
					<xsl:with-param name="unvisitedPoints" select="./@unvisitedPoints" />
					<xsl:with-param name="sequencePoints" select="./@sequencePoints" />
					<xsl:with-param name="coverage" select="./@coverage" />
					<xsl:with-param name="threshold" select="$threshold" />
				</xsl:call-template>
			</xsl:for-each>
		</xsl:for-each>
	</xsl:template>
		
	<!-- Namespaces Summary -->
	<xsl:template name="namespaceSummary">
		<xsl:param name="threshold" />
				<tr>
					<th class="primaryTable mainTableHeaderLeft">Namespaces</th>
					<th class="primaryTable mainTableHeader">Unvisited</th>
					<th class="primaryTable mainTableGraphHeader" colspan="2">Coverage</th>
				</tr>				
		<xsl:for-each select="//coverageReport/namespaces/namespace">
			<xsl:call-template name="coverageDetail">
				<xsl:with-param name="name" select="./@name" />
				<xsl:with-param name="unvisitedPoints" select="./@unvisitedPoints" />
				<xsl:with-param name="sequencePoints" select="./@sequencePoints" />
				<xsl:with-param name="coverage" select="./@coverage" />
				<xsl:with-param name="threshold" select="$threshold" />
			</xsl:call-template>
		</xsl:for-each>
	</xsl:template>
	
	<!-- Coverage detail row in main grid displaying a name, statistics and graph bar -->
	<xsl:template name="coverageDetail">
		<xsl:param name="name" />
		<xsl:param name="unvisitedPoints" />
		<xsl:param name="sequencePoints" />
		<xsl:param name="coverage" />
		<xsl:param name="threshold" />
				<tr>
					<td class="mainTableCellBottom mainTableCellItem"><xsl:value-of select="$name" /></td>
					<td class="mainTableCellBottom mainTableCellData"><xsl:value-of select="$unvisitedPoints" /></td>
					<td class="mainTableCellBottom mainTableCellPercent" align="right"><xsl:value-of select="concat(format-number($coverage,'#0'), '%')" /></td>
                    <td class="mainTableCellBottom mainTableCellGraph">
                        <xsl:call-template name="detailPercent">
                            <xsl:with-param name="notVisited" select="$unvisitedPoints" />
                            <xsl:with-param name="total" select="$sequencePoints" />
                            <xsl:with-param name="threshold" select="$threshold" />
                            <xsl:with-param name="scale" select="200" />
                        </xsl:call-template>
                    </td>
                </tr>
            </xsl:template>
	
	<!-- Coverage detail row in secondary grid header displaying a name, statistics and graph bar -->
	<xsl:template name="coverageDetailSecondary">
		<xsl:param name="name" />
		<xsl:param name="unvisitedPoints" />
		<xsl:param name="sequencePoints" />
		<xsl:param name="coverage" />
		<xsl:param name="threshold" />
				<tr>
					<td class="mainTableCellItem"><xsl:value-of select="$name" /></td>
					<td class="mainTableCellData"><xsl:value-of select="$unvisitedPoints" /></td>
					<td class="mainTableCellPercent"><xsl:value-of select="concat(format-number($coverage,'#0.0'), ' %')" /></td>
					<td class="mainTableCellGraph">
						<xsl:call-template name="detailPercent">
							<xsl:with-param name="notVisited" select="$unvisitedPoints" />
							<xsl:with-param name="total" select="$sequencePoints" />
							<xsl:with-param name="threshold" select="$threshold" />
							<xsl:with-param name="scale" select="200" />
						</xsl:call-template>
					</td>
				</tr>
	</xsl:template>
	
	<!-- Coverage detail row with indented item name and shrunk graph bar -->
	<xsl:template name="coverageIndentedDetail">
		<xsl:param name="name" />
		<xsl:param name="unvisitedPoints" />
		<xsl:param name="sequencePoints" />
		<xsl:param name="coverage" />
		<xsl:param name="threshold" />
				<tr>
					<td class="mainTableCellBottom childTableCellIndentedItem"><xsl:value-of select="$name" /></td>
					<td class="mainTableCellBottom mainTableCellData"><xsl:value-of select="$unvisitedPoints" /></td>
					<td class="mainTableCellBottom mainTableCellPercent"><xsl:value-of select="concat(format-number($coverage,'#0.0'), ' %')" /></td>
					<td class="mainTableCellBottom mainTableCellGraph">
						<xsl:call-template name="detailPercent">
							<xsl:with-param name="notVisited" select="$unvisitedPoints" />
							<xsl:with-param name="total" select="$sequencePoints" />
							<xsl:with-param name="threshold" select="$threshold" />
							<xsl:with-param name="scale" select="170" />
						</xsl:call-template>
					</td>
				</tr>
	</xsl:template>
		
	<!-- Exclusions Summary -->
	<xsl:template name="exclusionsSummary">
				<tr>
					<td class="exclusionTable mainTableHeaderLeft">Excluded From Coverage Results</td>
					<td class="exclusionTable mainTableGraphHeader" colspan="3">All Code Within</td>
				</tr>
		<xsl:for-each select="//coverageReport/exclusions/exclusion">
				<tr>
					<td class="mainTableCellBottom exclusionTableCellItem"><xsl:value-of select="@name" /></td>
					<td class="mainTableCellBottom mainTableCellGraph" colspan="3"><xsl:value-of select="@category" /></td>
				</tr>
		</xsl:for-each>
	</xsl:template>
	
	<!-- Footer -->
    <xsl:template name="footer">
        <div id="Footer">
            <hr />
            <p>This report was generated thanks to <a href="http://ncoverexplorer.org/">NCoverExplorer</a> and its author(s).</p>
        </div>
    </xsl:template>
	
	<!-- Draw % Green/Red/Yellow Bar -->
	<xsl:template name="detailPercent">
		<xsl:param name="notVisited" />
		<xsl:param name="total" />
		<xsl:param name="threshold" />
		<xsl:param name="scale" />
		<xsl:variable name="visited" select="$total - $notVisited" />
		<xsl:variable name="coverage" select="$visited div $total * 100"/>
		<table cellpadding="0" cellspacing="0">
		<tbody>
			<tr>
				<xsl:if test="not ($visited=0)">
					<td class="graphBarVisited" height="14">
						<xsl:attribute name="width">
							<xsl:value-of select="format-number($coverage div 100 * $scale, '0')" />
						</xsl:attribute>.
					</td>
				</xsl:if>
		        <xsl:if test="not($notVisited=0)">
					<td height="14">
						<xsl:attribute name="class">
							<xsl:if test="$coverage &gt;= $threshold">graphBarSatisfactory</xsl:if>
							<xsl:if test="$coverage &lt; $threshold">graphBarNotVisited</xsl:if>
						</xsl:attribute>
						<xsl:attribute name="width">
							<xsl:value-of select="format-number($notVisited div $total * $scale, '0')" />
						</xsl:attribute>.
					</td>
				</xsl:if>
			</tr>
			</tbody>
		</table>
	</xsl:template>
</xsl:stylesheet>