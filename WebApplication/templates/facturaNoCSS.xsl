<?xml version="1.0" encoding="iso-8859-1"?>

<xsl:stylesheet version="1.0" 
xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:sii="http://www.sii.cl/SiiDte"
xmlns:func="http://exslt.org/functions" 
xmlns:local="http://custodium.com/local" 
xmlns:exsl="http://exslt.org/common" 
extension-element-prefixes="sii func exsl local">
  
	<xsl:decimal-format name="clp" decimal-separator="," grouping-separator="."/>
	<xsl:decimal-format name="us" decimal-separator="." grouping-separator=","/>
	
 <!-- ============================ -->
 <!--  SOPORTE PARA CA4WEB INICIO  -->
 <!-- ============================ -->

 <!--  ESPECIFICACION DEL FORMATO Y ENCODING DE SALIDA   -->
 <xsl:output method="html" encoding="UTF-8" />

 <xsl:param name="custodium.ca4web" select="false()"/>
 <xsl:param name="custodium.document.href" select="''"/>
 <xsl:param name="custodium.stylesheet.href" select="''"/>

 <!-- ============================ -->
 <!-- SOPORTE PARA CA4WEB FIN ==== -->
 <!-- ============================ -->
 
	
	

<xsl:variable name="fechaEm" select="DTE/Documento/Encabezado/IdDoc/FchEmis"/>
<xsl:variable name="FechaHora" select="DTE/Documento/TmstFirma"/>
<xsl:variable name="Ano" select="substring($FechaHora,3,2)"/>
<xsl:variable name="Mes" select="substring($FechaHora,6,2)"/>
<xsl:variable name="Domain" select="/Document/@Domain"/>
<xsl:variable name="UrlFondo" select="concat('http://',$Domain ,$Ano ,$Mes ,'.acepta.com/styles/dte/images/fondo.gif')"/>
<xsl:variable name="URL" select="concat('http://',$Domain, $Ano, $Mes, '.acepta.com/')"/>
<!--xsl:variable name="fechaEmNum" select="concat(substring($fechaEm,0,5),substring($fechaEm,6,2), substring($fechaEm,9,2))"/--> 



  <xsl:template name="code39">
  <xsl:param name="height" />
  <xsl:param name="data" />
  <xsl:param name="module-width" />
  <xsl:choose>
   <xsl:when test="$custodium.ca4web">
    <xsl:variable name="width">
     <xsl:choose>
      <xsl:when test="substring($data, 1, 1) = '*'">
       <xsl:value-of select="format-number(($module-width * 2.15 * 13 *  string-length($data)     ) div 1000, '0.0', 'us')" />
      </xsl:when>
      <xsl:otherwise>
       <xsl:value-of select="format-number(($module-width * 2.15 * 13 * (string-length($data) + 3)) div 1000, '0.0', 'us')" />
      </xsl:otherwise>
     </xsl:choose>
    </xsl:variable>
    <!-- w2n height density data -->
    
   </xsl:when>
   <xsl:otherwise>
    <object id='code39' classid='clsid:05599D5C-30D6-49E9-8B31-6F08A23CF897' codebase='http://ws02.acepta.com/cabs/custodium.cab#version=1,0,0,0' style="width: {$module-width}cm; height: {$height}cm">
     <param name='DataToEncode' value='{$data}'/>
     <param name='ModuleWidth' value='11'/>
    </object>
   </xsl:otherwise>
  </xsl:choose>
 </xsl:template>
 
  <xsl:variable name="valor2_codigo_39">
	<xsl:call-template name="format_ean39">
	<!-- formatea cadena de caracteres para el codigo 39 -->
		
		<xsl:with-param name="Inicio"  select="'000'"/>
		<xsl:with-param name="Referencia"  select="DatoAdjunto[@nombre='NumRef']"/>
		<xsl:with-param name="Codigo"  select="DTE/Documento/Encabezado/Receptor/CdgIntRecep" />
	</xsl:call-template>
 </xsl:variable>
 
 
  <xsl:template name="padd0">
			<xsl:param    name="texto" />
			<xsl:param    name="size"  />
			<xsl:value-of select="substring('00000000000000000000', 1, $size - string-length($texto))" />
			<xsl:value-of select="$texto" />
  </xsl:template>

	<xsl:template name="format_ean39">
	
	<!-- formatea cadena de caracteres para el codigo 39 -->
	<xsl:param    name="Inicio" />
	<xsl:param    name="Referencia" />
	<xsl:param    name="Codigo" />
	
	<xsl:call-template name="padd0">
	<xsl:with-param name="texto" select="$Inicio" />
	<xsl:with-param name="size"  select="2" />
	</xsl:call-template>
	
	<xsl:call-template name="padd0">
	<xsl:with-param name="texto" select="$Referencia" />
	<xsl:with-param name="size"  select="2" />
	</xsl:call-template>
	
	<xsl:call-template name="padd0">
	<xsl:with-param name="texto" select="$Codigo" />
	<xsl:with-param name="size"  select="2" />
	</xsl:call-template>
  </xsl:template>
  
  <func:function name="local:xml_escape">
    	<xsl:param    name="input" />
    	<xsl:variable name="first" select="substring($input, 1, 1)" />
    	<xsl:variable name="tail"  select="substring($input, 2)"    />
    	<func:result>
    	    <xsl:choose>
    	    	<xsl:when test="$first = '&amp;' ">&amp;amp;</xsl:when>
    	    	<xsl:when test="$first = '&gt;'  ">&amp;gt;</xsl:when>
    	    	<xsl:when test="$first = '&lt;'  ">&amp;lt;</xsl:when>
    	    	<xsl:when test="$first = '&quot;'">&amp;quot;</xsl:when>
    	    	<xsl:when test='$first = "&apos;"'>&amp;apos;</xsl:when>
    	    	<xsl:otherwise><xsl:value-of select="$first" /></xsl:otherwise>
    	    </xsl:choose>
    	    <xsl:if test="$tail">
    	    	<xsl:value-of select="local:xml_escape($tail)" />
    	    </xsl:if>
    	</func:result>
      </func:function>

  
	<xsl:template match="/">
		<html>
			<head>
			  <title>.....</title>
        <meta http-equiv="X-UA-Compatible" content="IE=edge" />
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"></meta>
				<meta http-equiv="Pragma" content="no-cache"></meta>
				<meta http-equiv="Expires" content="-1" ></meta>
				<style type="text/css">	

.DTEPage {
  background-image   : url("http://gs.pso.cl/img/Fac_Fonodo.jpg"); 
  background-position: center;
  background-repeat  : repeat;
  position           : relative;
  width              : 195mm;
  height             : 100%;
  text-valign	 	 :top;
  size: A4;
  margin: 0mm 0mm 0mm 0mm;
  
}


.DTEidTable {
  border: 1mm solid FC0B0B;
  width: 8cm;
  height: 4cm;
  font-family: arial;
  font-size: 0.5cm;
  font-weight: bold; 
  color: FC0B0B;
  text-align: center;
}

.letraRazon {
  font-style  : normal;
  font-size   : 7mm;
  font-family : Arial, Helvetica;
  font-weight : bold;
  color       : black;
}

.letraGiro {
  font-style  : normal;
  font-size   : 3mm;
  font-family : Arial, Helvetica;
  color       : black;
}

.letraDireccion {
  font-style  : normal;
  font-size   : 3mm;
  font-family : Arial, Helvetica;
  color       : black;
}

.tablaFactura {
  border-style: solid;
  border-color: red;
  width	      : 70%;
}

.tablaDatos {
  border-style: solid;
  border-width: 1;
  border-color: rgb(49,66,132);
 
}

.cabeceraClientes {
  font-family : arial;
  font-size   : 8px;
  text-align  : left;
}

.cabeceraClientes2 {
  font-family : arial;
  font-size   : 8px;
  text-align  : center;
}

.textoClientes {
  font-family : arial;
  font-size   : 8px;
  text-align  : left;
}

.textoTransporte {
  font-family : arial;
  font-size   : 8px;
  text-align  : left;  
}

.textoReferencia {
  font-family : arial;
  font-size   : 12px;
  text-align  : left;
}

.cabeceraDetallesB {
  border-right  : 1px solid rgb(49,66,132);
  font-family   : arial;
  font-size     : 8px;
  font-weight   : bold;
  text-align    : center;
  border-bottom : 1px solid rgb(49,66,132);
  border-top    : 1px solid rgb(49,66,132);
  color         : black;  
}

.cabeceraDetallesN {
  font-family   : arial;
  font-size     : 10px;
  text-align    : center;
  border-bottom : 1px solid rgb(49,66,132);
  border-top    : 1px solid rgb(49,66,132);
  color         : rgb(49,66,132);
}

.DetalleB {
  font-family  : arial;
  font-size    : 8px;
  border-right : 1px solid rgb(49,66,132);
}

.DetalleN {
  font-family  : arial;
  font-size    : 10px;
}

.productoTextoConMargenDF {
  font-family :arial;
  font-size   :10px;
  border-right:1px solid rgb(49,66,132);
  border-bottom:1px solid rgb(49,66,132);
}

.TablaTotales {
  font-family :arial;
  font-size   :9px;
  font-weight :bold;
  text-align  :left;
  border      :2px solid rgb(49,66,132);
  color       :rgb(49,66,132);
}

.TotalGlosa {
  font-family :Arial;
  font-size   :2.5mm;
  font-weight :bold;
  text-align  :left;
  vertical-align: bottom;
  color       :rgb(49,66,132);
}

.TotalMonto {
  font-family :Courier New;
  font-size   :3.5mm;
  font-weight :bold;
  text-align  :right;
  color       :black;
}

.espacio {
  height: 1mm;
  font-size: 1mm;
}

.oficina-sii {
  font-family: arial; 
  font-size: 3mm; 
  color: red; 
  text-align: center; 
  font-weight: bold; 
  text-transform: uppercase;
}
<!-- CAmbios!-->
/* ESTILO PARA COLOCAR ESPACIO (DE ALTO ARBITRARIO) A LAS FILAS O LINEAS DE UNA TABLA */
.espacioLinea {
  line-height: 7pt;
}


/* CUADRADO ROJO ESQ SUP DER*/

.tablaFactura {
 border: 1mm solid red;
  width: 6cm;
  height: 3cm;
  font-family: arial;
  font-size: 0.4cm;
  font-weight: bold;
  color: red;
  text-align: center;
}

.upper { text-transform: uppercase; }
 <!-- __________________ TIMBRE ________________ -->
				
#Timbre {padding: 10px 0px 0px 0px; font-family: sans-serif;	font-size:8pt; float: left;margin-left; width: 350px;	border:1px none #414040;	z-index:3;}				
#textoTimbre {padding: 0px 0px 0px 0px; font-family: sans-serif;	font-weight: bold;	font-size:7pt;	width: 350px;	float: left;margin-left;	border:1px none #414040;	text-align: center;}				
#TimbreResolucion {padding: 0px 0px 0px 0px; font-family: sans-serif;	font-size:8pt; float: left;margin-left;	width: 320px;}				
#textoResolucion {padding: 0px 0px 0px 0px; font-family: sans-serif; font-weight: bold;	font-size:7pt;	width: 420px;	float: left;margin-left;	border:1px none #414040; text-align: center;}
												 			      

<!-- __________________ CODIGO DE BARRA _________________________ -->
				
				#code39 { width: 3in; height: 0.25in; }
				
				#CodigoBarra {padding: 3px 0px 0px 0px; font-family: sans-serif;	font-size:8pt; float: left;margin-left;	width: 250px;	border:1px none #414040; z-index:3;}				
				#textoBarra {padding: 3px 0px 0px 0px; font-family: sans-serif; font-weight: bold;	font-size:7pt;	width: 300px;	float: left;margin-left;	border:1px none #414040;}						
				
</style>
	<script src="http://192.168.81.89:8082/Scripts/pdf417-js-master/bcmath-min.js" type="text/javascript"></script>
  <script src="http://192.168.81.89:8082/Scripts/NumeroALetras.js" type="text/javascript"></script>
  <script src="http://192.168.81.89:8082/Scripts/pdf417-js-master/pdf417-min.js" type="text/javascript"></script>
	
	

<script>

     <xsl:comment><![CDATA[	
	
	String.prototype.formRut = function() {
	result = this.toUpperCase();
	result = result.replace(/[^0-9K]+/g, "");
	result = result.match(/(.+)(...)(...)(.)/);
	document.write(result[1] + "." + result[2] +  "." + result[3] +  "-" + result[4]);
	}	

	String.prototype.formDTEName = function () {
	switch (this + '.') {
	case '30.': result = "FACTURA TRADICIONAL"; break;
	case '33.': result = "FACTURA ELECTRONICA"; break;
	case '34.': result = "FACTURA NO AFECTA <br/>O EXENTA ELECTRONICA"; break;
	case '32.': result = "FACTURA NO AFECTA <br/>O EXENTA "; break;
	case '45.': result = "FACTURA COMPRA"; break;
	case '50.': result = "GUIA DE DESPACHO"; break;
	case '52.': result = "GUÍA DE DESPACHO <br/>ELECTRONICA"; break;
	case '55.': result = "NOTA DE DEBITO"; break;
	case '56.': result = "NOTA DE DEBITO <br/>ELECTRONICA"; break;
	case '60.': result = "NOTA DE CREDITO"; break;
	case '61.': result = "NOTA DE CREDITO <br/>ELECTRONICA"; break;
	case 'SET.': result = "SET DE PRUEBA"; break;
	default:    result = "¿Tipo DTE? ["+this+"]"; break;
	}
	document.write(result);
	}

	window.onload = function () { create417() }
	
	
	
	
	function create417() {
            var textToEncode = document.getElementsByName("DataToEncode");

            PDF417.init(textToEncode[0].value);             

            var barcode = PDF417.getBarcodeArray();

            // block sizes (width and height) in pixels
            var bw = 1;
            var bh = 1;

            // create canvas element based on number of columns and rows in barcode
            var container = document.getElementById('pdf417');
            container.removeChild(container.firstChild);

            var canvas = document.createElement('canvas');
            canvas.width = bw * barcode['num_cols'];
            canvas.height = bh * barcode['num_rows'];
            container.appendChild(canvas);

            var ctx = canvas.getContext('2d');                    

            // graph barcode elements
            var y = 0;
            // for each row
            for (var r = 0; r < barcode['num_rows']; ++r) {
                var x = 0;
                // for each column
                for (var c = 0; c < barcode['num_cols']; ++c) {
                    if (barcode['bcode'][r][c] == 1) {                        
                        ctx.fillRect(x, y, bw, bh);
                    }
                    x += bw;
                }
                y += bh;
            }
	}
	
	
	String.prototype.formFecha = function() {
	if (this != "") {
	document.write(this.substring(8,10) + "/" + this.substring(5,7) + "/" + this.substring(0,4));
	}
	}	
  ]]></xsl:comment>
  	
	
</script>
			</head>
			<body style="width: 215mm;margin: 0px;margin-left:0px;margin-right:0px; text-align: center; background-color: white;">
				<table class="DTEPage" border="0">
					<tr>
						<td valign="top">
                           
    <!--=================================================TABLA ENCABEZADO==========================================-->                                 

                                    <!-- ADMINISTRADORA CENTROS COMERCIALES ALTO LAS CONDES-->

  <table width="99%"  border="0" cellpadding="0" cellspacing="0">
   <tr>

     <td width="36%" valign="top">
       <br/>
	  <table style="width: 97%;"  border="0" cellpadding="0" cellspacing="0">
      
       <tr>
         <td><span class="letraGiro"><b><xsl:value-of select="DTE/Documento/Encabezado/Emisor/RznSoc"/></b><BR/>
			<xsl:value-of select="DTE/Documento/Encabezado/Emisor/GiroEmis"/><BR/>
			<b>Casa Matriz:</b><BR/>
			<xsl:value-of select="DTE/Documento/Encabezado/Emisor/DirOrigen"/><br/>
			<xsl:value-of select="DTE/Documento/Encabezado/Emisor/CmnaOrigen"/> - <xsl:value-of select="DTE/Documento/Encabezado/Emisor/CiudadOrigen"/> - Chile<br/>

			</span>
			</td>
       </tr>
     </table>
	 
	 </td>

	 
   <td width="41%" valign="top">
	  <table width="100%"     align="right" border="0" cellpadding="0" cellspacing="0">
       <tr>
         <td align="right">
		    <table border="0"   align="right" class="tablaFactura" height="140" valign="top">
				<tr>
				<td align="center">
				<font face="arial" size="2" color="red">
				<b>
				R.U.T.:
				<xsl:value-of select='DTE/Documento/Encabezado/Emisor/RUTEmisor'/>
				<br/>
				<div class="espacioLinea">&#160;</div>
                <div>
				<xsl:for-each select = "DTE/Documento/Encabezado/IdDoc"> 	
					<xsl:if test = "TipoDTE = 30">	 
						<div>FACTURA TRADICIONAL</div>
					</xsl:if> 
					<xsl:if test = "TipoDTE = 33">	 
						<div>FACTURA ELECTRONICA</div>
					</xsl:if> 
					<xsl:if test = "TipoDTE = 34">	 
						<div>FACTURA NO AFECTA <br/> EXENTA ELECTRONICA</div>
					</xsl:if> 
					<xsl:if test = "TipoDTE = 32">	 
						<div>FACTURA NO AFECTA <br/> EXENTA</div>
					</xsl:if>
					<xsl:if test = "TipoDTE = 45">	 
						<div>FACTURA COMPRA</div>
					</xsl:if> 	
					<xsl:if test = "TipoDTE = 50">	 
						<div>GUIA DE DESPACHO</div>
					</xsl:if>
					<xsl:if test = "TipoDTE = 52">	 
						<div>GUÍA DE DESPACHO <br/>ELECTRONICA</div>
					</xsl:if> 						
					<xsl:if test = "TipoDTE = 55">	 
						<div>NOTA DE DEBITO</div>
					</xsl:if> 	
					<xsl:if test = "TipoDTE = 56">	 
						<div>NOTA DE DEBITO <br/>ELECTRONICA</div>
					</xsl:if> 						
					<xsl:if test = "TipoDTE = 60">	 
						<div>NOTA DE CREDITO</div>
					</xsl:if> 	
					<xsl:if test = "TipoDTE = 61">	 
						<div>NOTA DE CREDITO <br/>ELECTRONICA</div>
					</xsl:if> 							
               </xsl:for-each> 
				</div>
			    <div class="espacioLinea">&#160;</div>
			Nº <xsl:value-of select="DTE/Documento/Encabezado/IdDoc/Folio"/>
			<br/>
			</b>
			</font>
			</td>
			</tr>
			</table>
		</td>
       </tr>	   
	   <tr>
	 
	   <td align="center"><!--<font face="arial" size="2" color="red"><b>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;S.I.I - SANTIAGO ORIENTE</b></font>&#160;&#160;&#160;&#160;&#160;--></td>
	   <td>&#160;</td>
       </tr>	   
       <tr>
         <td align="center"><b><font style="font-family:arial;font-size:4.8pt;">
          <xsl:value-of select="DatoAdjunto[@nombre='TituloFactura']"/>&#160;<xsl:value-of select="DatoAdjunto[@nombre='numFolioSap']"/><br/>
		 <xsl:value-of select="DatoAdjunto[@nombre='NumFacSap']"/>
	     </font></b></td>
       </tr>       
     </table></td>
   </tr>
 </table>
<!--=================================================FIN TABLA ENCABEZADO==========================================-->
						 
							<!--COMIENZO tabla datos receptor-->                         
                          
						<xsl:choose>
						 <xsl:when test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
							<table  style="width: 99%;" cellspacing="0" cellpadding="0" border="0">
								<tr style="height: 2mm;">
								    <td style="width:2mm;border-left:1px solid navy;border-top:1px solid navy;"></td>
									<td style="width: 1.5cm;border-top:1px solid navy;" class="cabeceraClientes">SEÑOR(es):</td>
									<td class="textoClientes upper" style="width: 3.5cm;border-top:1px solid navy;"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/RznSocRecep"/></td>
									<td style="width: 1.5cm;border-top:1px solid navy;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;border-top:1px solid navy;" class="cabeceraClientes">&#160;</td>
									<td style="width: 3.5cm;border-top:1px solid navy;" class="cabeceraClientes">FECHA DE EMISION:</td>
									<td style="width: 2.8cm; text-align: left;border-top:1px solid navy;" class="textoClientes"><xsl:value-of select='DTE/Documento/Encabezado/IdDoc/FchEmis'/></td>
									<td style="width: 0.1cm;"></td>									
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">R.U.T.:</td>
									<td class="textoClientes"><xsl:value-of select='DTE/Documento/Encabezado/Receptor/RUTRecep'/>
									  &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;
									  &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;									  
									  TELEFONO: &#160;<xsl:value-of select="DatoAdjunto[@nombre='Telefono']"/>
									</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>                                                                 
									 <td style="width: 3.5cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='Texto1']"/>&#160;</td>
									<td style="width: 2.8cm; text-align: right;" class="textoClientes">&#160;</td>									 
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">GIRO:</td>
									<td class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/GiroRecep"/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 3.5cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumEntrega']"/></td>
									<td style="width: 2.8cm; text-align: right;"  class="textoClientes">&#160;</td>
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">DIRECCION:</td>
									<td class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/DirRecep"/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 3.5cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumPedSap']"/></td>
									<td style="width: 2.8cm; text-align: right;" class="textoClientes">&#160;</td>
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">COMUNA:</td>
									<td class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CmnaRecep"/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">CIUDAD:</td>
									<td style="width: 1.5cm;" class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CiudadRecep"/>&#160;</td>						           
									<td class="cabeceraClientes">&#160;<xsl:value-of select="DatoAdjunto[@nombre='DescCentroOrigen']"/>&#160;</td>									
									<td class="cabeceraClientes">&#160;<xsl:value-of select="DatoAdjunto[@nombre='DescCentroDestino']"/>&#160;</td>
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
                                     <td style="width: 0.1cm;border-top:1px solid navy;border-right:1px solid navy;"></td>
                                     <td colspan="6" style="border-top:1px solid navy;border-bottom:1px solid navy " >
									 <span class="TextoClientes">
	                                    <font  class="cabeceraClientes">DOCUMENTO REFERENCIA:&#160;</font>
										<!-- <xsl:if   test="DTE/Documento/Referencia and DTE/Documento/Referencia/IndGlobal[.!='1']">-->
										   <xsl:for-each select="DTE/Documento/Referencia">
										      <xsl:value-of select="RazonRef"/>&#160;<script>"<xsl:value-of select="TpoDocRef"/>".formDTEName()</script>&#160; con  Folio <xsl:value-of select='format-number(FolioRef, "###.##0", "clp")'/> y  fecha de emisión <xsl:value-of select="FchRef"/>
                                           </xsl:for-each>
										   <!-- </xsl:if>-->
                                         </span>
                                        &#160;
                                      </td> 
                                      <td style="width: 0.1cm;border-top:1px solid navy"></td>
                                   </tr>
							      </table>
								  </xsl:when>
                                  <xsl:otherwise>
                                     <table  style="width: 99%;" cellspacing="0" cellpadding="0" border="0">
								<tr style="height: 2mm;">
								    <td style="width: 2mm;border-left:1px solid navy;border-top:1px solid navy;"></td>
									<td style="width: 1.5cm;border-top:1px solid navy;" class="cabeceraClientes ">SEÑOR(es):</td>
									<td class="textoClientes upper" style="width: 20cm; border-top:1px solid navy;"><xsl:value-of select="/DTE/Documento/Encabezado/Receptor/RznSocRecep"/></td>
									<td style="width: 1.5cm;border-top:1px solid navy;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;border-top:1px solid navy;" class="cabeceraClientes">&#160;</td>
									<td style="width: 3.5cm;border-top:1px solid navy;" class="cabeceraClientes">FECHA DE EMISION:</td>
									<td style="width: 7.1cm; text-align: center;border-top:1px solid navy;" class="textoClientes"><xsl:value-of select='DTE/Documento/Encabezado/IdDoc/FchEmis'/></td>
									<td style="width: 0.1cm;border-right:1px solid navy;border-top:1px solid navy;"></td>									
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">R.U.T.:</td>
									<td class="textoClientes"><xsl:value-of select='DTE/Documento/Encabezado/Receptor/RUTRecep'/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
                                                                 <xsl:choose>
								    <xsl:when test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
									 <td style="width: 3.5cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='CondVenta']"/>&#160;</td>
									<td style="width: 2.8cm; text-align: right;" class="textoClientes">&#160;</td>
									 </xsl:when>
									 <xsl:otherwise>
									<td style="width: 3.5cm;" class="cabeceraClientes">FECHA DE VENC:</td>
									<td style="width: 2.8cm; text-align: right;" class="textoClientes"><script>"<xsl:value-of select='DTE/Documento/Encabezado/IdDoc/FchVenc'/>".formFecha()</script></td>                    
									</xsl:otherwise>
									</xsl:choose>
									<td style="width: 0cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">GIRO:</td>
									<td class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/GiroRecep"/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes"></td>
									<td style="width: 3.5cm;" class="cabeceraClientes">FORMA DE PAGO&#160;</td>
									<td style="width: 2.8cm; text-align: right;"  class="textoClientes"><xsl:value-of select="DatoAdjunto[@nombre='DescCondPago']"/>&#160;</td>
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">DIRECCION:</td>
									<td class="textoClientes upper"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/DirRecep"/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">&#160;</td>
									<td style="width: 3.5cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='Texto1']"/>&#160;</td>
									<td style="width: 2.8cm; text-align: right;" class="textoClientes">&#160;</td>
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
								    <td style="width: 0.1cm;border-left:1px solid navy;">&#160;</td>
									<td style="width: 1.5cm;" class="cabeceraClientes">COMUNA:</td>
									<td class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CmnaRecep"/></td>
									<td style="width: 1.5cm;" class="cabeceraClientes">CIUDAD:</td>
									<td style="width: 1.5cm;" class="textoClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CiudadRecep"/>&#160;</td>						           
									  <td style="width: 2.8cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumPedSap']"/>&#160;</td>									
									<td style="width: 1.5cm;" class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='OfVta']"/>&#160;</td>
									<td style="width: 0.1cm; border-right:1px solid navy">&#160;</td>
								</tr>
								<tr style="height: 2mm;">
                                     <td style="width: 0.1cm;border-top:1px solid navy;border-left:1px solid navy;border-bottom:1px solid navy;"></td>
                                     <td colspan="6" style="border-top:1px solid navy;border-bottom:1px solid navy " >
									     <span class="TextoClientes">
											<!-- <font  class="cabeceraClientes">DOCUMENTO REFERENCIA:&#160;</font> -->
											<!-- <xsl:if   test="DTE/Documento/Referencia and DTE/Documento/Referencia/IndGlobal[.!='1']">									
											   <xsl:for-each select="DTE/Documento/Referencia">
												  <xsl:value-of select="CodRef"/>&#160;<script>"<xsl:value-of select="TpoDocRef"/>".formDTEName()</script>&#160; con  Folio <xsl:value-of select='format-number(FolioRef, "###.##0", "clp")'/> y  fecha de emisión <xsl:value-of select="FchRef"/>
											   </xsl:for-each>
												</xsl:if>--> 
                                         </span>
                                        &#160;
                                      </td> 
                                      <td style="width: 0.1cm;border-top:1px solid navy;border-right:1px solid navy;border-bottom:1px solid navy;"></td>
                                   </tr>
							      </table>

								  </xsl:otherwise>  
								  </xsl:choose>
	   
						
							<div align="center"><font  class="cabeceraClientes">A: <xsl:value-of select="DTE/Documento/Encabezado/Emisor/RznSoc"/></font></div>
				
							<!--fin de tabla con datos receptor, empieza tabla detalle -->
							<table border="0" cellspacing="0" cellpadding="0" width="100%">
								<tr>
									<td>
									<xsl:choose>
									 <xsl:when test="DatoAdjunto[@nombre='CodImpresion']= '331' or DatoAdjunto[@nombre='CodImpresion']= '561' or DatoAdjunto[@nombre='CodImpresion']= '341'">   
										<!-- INICIO de tabla contenido con colilla -->                                     
										<table border="0" style="width: 99%; border-left: 1px solid navy; border-bottom: 1px solid navy;" cellspacing="0">
											<tr >												
												<td class="cabeceraDetallesB" style="width: 2cm; src=http://www.custodium.com/docs/tempresas/factura/img/s_i_g.gif">CODIGO</td>
												<td class="cabeceraDetallesB" style="width: 1cm;">CANT.</td>
												<td class="cabeceraDetallesB">DESCRIPCION</td>												
												<td class="cabeceraDetallesB" style="width: 2cm;">PRECIO UNIT OTRA MONEDA</td>
												<td class="cabeceraDetallesB" style="width: 2cm;">PRECIO UNIT PESOS</td>
												<td class="cabeceraDetallesB" style="width: 2cm;">VALOR</td>
											</tr>
											<tr >												
												<td class="DetalleB" >&#160;</td>
												<td class="DetalleB" >&#160;</td>
												<td class="DetalleB" >&#160;</td>
												<td class="DetalleB" >&#160;</td>
												<td class="DetalleB" >&#160;</td>
												<td class="DetalleB" >&#160;</td>
											</tr>
											<xsl:for-each select="DTE/Documento/Detalle">											 
											 <xsl:if test="DscItem[.!= '']">												
											   <tr >
											    <td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>												
												<td class="DetalleB" align="left" valign="top">&#160;
												  <xsl:value-of select="DscItem"/>												  
												</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												</tr>
												</xsl:if>
												<tr >												
												 <td class="DetalleB" align="left" valign="top">
													<xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="''"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 														
													 	 <xsl:if test="CdgItem/VlrCodigo[.!='']"><xsl:value-of select="CdgItem/VlrCodigo"/></xsl:if>
													   </xsl:otherwise>
													  </xsl:choose>&#160;	
													</td>
													<td class="DetalleB" align="center" valign="top">
													  <xsl:choose>
													     <xsl:when test="./IndExe = 4"> 
														  <xsl:value-of select="''"/>
                                                         </xsl:when>
                                                         <xsl:otherwise> 														
															<xsl:if test="QtyItem[.!='']"><xsl:value-of select="QtyItem"/></xsl:if>
														 </xsl:otherwise>	
														</xsl:choose>&#160;
													</td>
													<td class="DetalleB" align="left" valign="top">&#160;
													  <xsl:choose>
													    <xsl:when test="./IndExe = 4"> 
														  <xsl:value-of select="''"/>
                                                         </xsl:when>
                                                         <xsl:otherwise> 
													      <xsl:value-of select="NmbItem"/>
                                                         </xsl:otherwise>
                                                        </xsl:choose>
													</td>													
													<td class="DetalleB" align="right" valign="top">
													<xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="''"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 		
													 <xsl:if test="OtrMnda/PrcOtrMon">
													   <xsl:value-of select='format-number(OtrMnda/PrcOtrMon, ".##0,##", "clp")'/>&#160;
													   <xsl:value-of select="OtrMnda/Moneda" />
													</xsl:if> 
													</xsl:otherwise>
													</xsl:choose>
													&#160;
													</td>
													<td class="DetalleB" align="right" valign="top">
													 <xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="''"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 		
														<xsl:if test="PrcItem[.!='']">
																<xsl:value-of select='format-number(PrcItem, "###.##0", "clp")'/>
													</xsl:if>
													</xsl:otherwise>
													</xsl:choose>&#160;
													</td>													
													<td class="DetalleB" align="right" valign="top">
													 <xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="' '"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 	
													 <xsl:if test="MontoItem != ''">
													  <xsl:value-of select='format-number(MontoItem, "###.##0", "clp")'/>
													 </xsl:if>
													 </xsl:otherwise>
													 </xsl:choose>&#160;</td>
												</tr>
											</xsl:for-each>
											
											<tr>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
                                 
                                  
									<td class="DetalleB" valign="bottom">								 
									<xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
								     <table  style="width: 99%;" cellspacing="0" cellpadding="0" border="0">								  
									 
								      <tr>
								       <td style="width: 3cm;"><span class="textoTransporte">&#160;Patente:&#160;</span></td>
									   <xsl:if test="DTE/Documento/Encabezado/Transporte/Patente"> 
								       <td  style="font-family:arial;font-size:6pt;"><xsl:value-of select = "DTE/Documento/Encabezado/Transporte/Patente" />&#160;</td>
									   </xsl:if>
								      </tr>
									  
									  
								      <tr>
								       <td style="width: 3cm;"><span class="textoTransporte">&#160;R.U.T. Transportista:&#160;</span></td>
									   <xsl:if test="DTE/Documento/Encabezado/Transporte/RUTTrans"> 
								       <td style="font-family:arial;font-size:6pt;"><script>"<xsl:value-of select='DTE/Documento/Encabezado/Transporte/RUTTrans'/>".formRut()</script>&#160;</td>
									   </xsl:if>
								     </tr>
									 
									 
								     <tr>
								      <td style="width: 3cm;"><span class="textoTransporte">&#160;Direccion:&#160;</span></td>
									  <xsl:if test="DTE/Documento/Encabezado/Transporte/DirDest"> 
								      <td style="font-family:arial;font-size:6pt;"><xsl:value-of select = "DTE/Documento/Encabezado/Transporte/DirDest" />&#160;</td>
									  </xsl:if>
								     </tr>
									   
									 
								     <tr>
								      <td style="width: 3cm;"><span class="textoTransporte">&#160;Comuna:&#160;</span></td>
									  <xsl:if test="DTE/Documento/Encabezado/Transporte/CmnaDest"> 
								      <td style="font-family:arial;font-size:6pt;"><xsl:value-of select = "DTE/Documento/Encabezado/Transporte/CmnaDest" />&#160;</td>
									  </xsl:if>
								     </tr>
									 
								    </table>
									</xsl:if>
									 &#160;
									</td>
									
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
											</tr>	
                                          <xsl:if test="DTE/Documento/DscRcgGlobal/ValorDR">
											<tr style="height: 1mm;">
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB" valign="bottom">												
												   <b><xsl:value-of select="DTE/Documento/DscRcgGlobal/GlosaDR"/>&#160;</b>												 
												&#160;</td>												
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB"><xsl:value-of select='format-number(DTE/Documento/DscRcgGlobal/ValorDR, ".##0,00", "clp")'/>&#160;</td>
											</tr>
											</xsl:if>									
											<tr style="height: 1mm;">
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB" valign="bottom">
                                               <xsl:choose> 
												<xsl:when test="DTE/Documento/Detalle/OtrMnda/FctConv and DTE/Documento/Detalle/OtrMnda/Moneda[.='USD'] ">
												   <b>El Tipo de Cambio es 1 <xsl:value-of select="DTE/Documento/Detalle/OtrMnda/Moneda" />&#160;</b><xsl:value-of select='format-number(DTE/Documento/Detalle/OtrMnda/FctConv, ".##0,00", "clp" )'/>
												 </xsl:when>
												 <xsl:otherwise>
												  <xsl:if test="DTE/Documento/Detalle/OtrMnda/FctConv and DTE/Documento/Detalle/OtrMnda/Moneda[.='UF']">
												   <b>El Tipo de Cambio es 1 <xsl:value-of select="DTE/Documento/Detalle/OtrMnda/Moneda" />&#160;</b><xsl:value-of select='format-number(DTE/Documento/Detalle/OtrMnda/FctConv, ".##0,00", "clp" )'/>
                                                  </xsl:if> 
												 </xsl:otherwise>
											    </xsl:choose>
												&#160;</td>												
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
											</tr>									
										</table>
										<!-- FIN tabla de contenido con colilla-->  
									</xsl:when>
									 <xsl:otherwise>
                                        <!-- INICIO de tabla contenido sin colilla-->                                     
										<!--<table border="0" style="width: 99%; height: 11.5cm; border-left: 1px solid navy; border-bottom: 1px solid navy;" cellspacing="0">-->
                     <table border="0" style="width: 99%; height: 7cm; border-left: 1px solid navy; border-bottom: 1px solid navy;" cellspacing="0">
											<tr style="height: 1mm;">												
												<td class="cabeceraDetallesB" style="width: 2cm; src=http://www.custodium.com/docs/tempresas/factura/img/s_i_g.gif">CODIGO</td>
												<td class="cabeceraDetallesB" style="width: 1cm;">CANT.</td>
												<td class="cabeceraDetallesB">DESCRIPCION</td>												
												<td class="cabeceraDetallesB" style="width: 2cm;">PRECIO UNIT OTRA MONEDA</td>
												<td class="cabeceraDetallesB" style="width: 2cm;">PRECIO UNIT PESOS</td>
												<td class="cabeceraDetallesB" style="width: 2cm;">VALOR</td>
											</tr>

											<xsl:for-each select="DTE/Documento/Detalle">											 
											 <xsl:if test="DscItem[.!= '']">												
											   <tr style="height: 0.5mm;" >
											    <td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>												
												<td class="DetalleB" align="left" valign="top">&#160;
												  <xsl:value-of select="DscItem"/>												  
												</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												</tr>
												</xsl:if>
												<tr style="height: 0.5mm;" >												
												 <td class="DetalleB" align="left" valign="top">
													<xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="''"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 														
													 	 <xsl:if test="CdgItem/VlrCodigo[.!='']">
														  <xsl:value-of select="CdgItem/VlrCodigo"/>
														 </xsl:if>
													   </xsl:otherwise>
													  </xsl:choose>&#160;	
													</td>
													<td class="DetalleB" align="center" valign="top">
													  <xsl:choose>
													     <xsl:when test="./IndExe = 4"> 
														  <xsl:value-of select="''"/>
                                                         </xsl:when>
                                                         <xsl:otherwise> 														
															<xsl:if test="QtyItem[.!='']">
															  <xsl:value-of select="QtyItem"/>
															</xsl:if>
														 </xsl:otherwise>	
														</xsl:choose>&#160;
													</td>
													<td class="DetalleB" align="left" valign="top">&#160;													  
													      <xsl:value-of select="NmbItem"/>                                                      
													</td>													
													<td class="DetalleB" align="right" valign="top">
													<xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="''"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 		
													 <xsl:if test="OtrMnda/PrcOtrMon">
													   <xsl:value-of select='format-number(OtrMnda/PrcOtrMon, ".##0,##", "clp")'/>&#160;
													   <xsl:value-of select="OtrMnda/Moneda" />
													</xsl:if> 
													</xsl:otherwise>
													</xsl:choose>
													&#160;
													</td>
													<td class="DetalleB" align="right" valign="top">
													 <xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="' '"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 		
														<xsl:if test="PrcItem[.!='']">
																<xsl:value-of select='format-number(PrcItem, "###.##0", "clp")'/>
													</xsl:if>
													</xsl:otherwise>
													</xsl:choose>&#160;
													</td>													
													<td class="DetalleB" align="right" valign="top">
													<xsl:choose>
													   <xsl:when test="./IndExe = 4"> 
													     <xsl:value-of select="' '"/>
                                                       </xsl:when>
                                                       <xsl:otherwise> 	
													 <xsl:if test="MontoItem != ''">
													  <xsl:value-of select='format-number(MontoItem, "###.##0", "clp")'/>
													 </xsl:if>
													 </xsl:otherwise>
													 </xsl:choose>
													 &#160;</td>
												</tr>
											</xsl:for-each>
											
											<tr>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
                                 
                                  
									<td class="DetalleB" valign="bottom">								 
									<xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
								     <table  style="width: 99%;" cellspacing="0" cellpadding="0" border="0">								  
									 
								      <tr>
								       <td style="width: 3cm;"><span class="textoTransporte">&#160;Patente:&#160;</span></td>
									   <xsl:if test="DTE/Documento/Encabezado/Transporte/Patente"> 
								       <td  style="font-family:arial;font-size:6pt;"><xsl:value-of select = "DTE/Documento/Encabezado/Transporte/Patente" />&#160;</td>
									   </xsl:if>
								      </tr>
									  
									  
								      <tr>
								       <td style="width: 3cm;"><span class="textoTransporte">&#160;R.U.T. Transportista:&#160;</span></td>
									   <xsl:if test="DTE/Documento/Encabezado/Transporte/RUTTrans"> 
								       <td style="font-family:arial;font-size:6pt;"><script>"<xsl:value-of select='DTE/Documento/Encabezado/Transporte/RUTTrans'/>".formRut()</script>&#160;</td>
									   </xsl:if>
								     </tr>
									 
									 
								     <tr>
								      <td style="width: 3cm;"><span class="textoTransporte">&#160;Direccion:&#160;</span></td>
									  <xsl:if test="DTE/Documento/Encabezado/Transporte/DirDest"> 
								      <td style="font-family:arial;font-size:6pt;"><xsl:value-of select = "DTE/Documento/Encabezado/Transporte/DirDest" />&#160;</td>
									  </xsl:if>
								     </tr>
									 
									 
								     <tr>
								      <td style="width: 3cm;"><span class="textoTransporte">&#160;Comuna:&#160;</span></td>
									  <xsl:if test="DTE/Documento/Encabezado/Transporte/CmnaDest"> 
								      <td style="font-family:arial;font-size:6pt;"><xsl:value-of select = "DTE/Documento/Encabezado/Transporte/CmnaDest" />&#160;</td>
									  </xsl:if>
								     </tr>
									 
								    </table>
									</xsl:if>
									 &#160;
									</td>
									
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
											</tr>	
                                          <xsl:if test="DTE/Documento/DscRcgGlobal/ValorDR">
											<tr style="height: 1mm;">
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB" valign="bottom">												
												   <b><xsl:value-of select="DTE/Documento/DscRcgGlobal/GlosaDR"/>&#160;</b>												 
												&#160;</td>												
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB"><xsl:value-of select='format-number(DTE/Documento/DscRcgGlobal/ValorDR, ".##0,00", "clp")'/>&#160;</td>
											</tr>
											</xsl:if>									
											<tr style="height: 1mm;">
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB" valign="bottom">
                                               <xsl:choose> 
												<xsl:when test="DTE/Documento/Detalle/OtrMnda/FctConv and DTE/Documento/Detalle/OtrMnda/Moneda[.='USD'] ">
												   <b>El Tipo de Cambio es 1 <xsl:value-of select="DTE/Documento/Detalle/OtrMnda/Moneda" />&#160;</b><xsl:value-of select='format-number(DTE/Documento/Detalle/OtrMnda/FctConv, ".##0,00", "clp" )'/>
												 </xsl:when>
												 <xsl:otherwise>
                                                  <xsl:if test="DTE/Documento/Detalle/OtrMnda/FctConv and DTE/Documento/Detalle/OtrMnda/Moneda[.='UF'] ">
												    <b>El Tipo de Cambio es 1 <xsl:value-of select="DTE/Documento/Detalle/OtrMnda/Moneda" />&#160;</b><xsl:value-of select='format-number(DTE/Documento/Detalle/OtrMnda/FctConv, ".##0,00", "clp" )'/>
                                                  </xsl:if>
												 </xsl:otherwise>
											    </xsl:choose>
												&#160;</td>												
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
												<td class="DetalleB">&#160;</td>
											</tr>									
										</table>
										<!-- FIN tabla de contenido sin colilla --> 
									  
									 </xsl:otherwise>
									</xsl:choose>
									</td>
								</tr>
                
                <tr>
                  <td>
                      <table  style="width: 99%;border:1px solid navy" cellspacing="0" cellpadding="0" border="0">
                        <tr >
                              <td style="width: 10cm; " align="left" class="cabeceraClientes"> Referencias:</td>
                              <td style="width: 6.8cm;font-weight: bold !important;border-left:1px solid navy; " align="left" class="cabeceraClientes"><b> Desglose de Impuestos:</b></td>
                        </tr>
                        
                          <tr>
                            <td class="cabeceraClientes">
                              <xsl:for-each select="DTE/Documento/Referencia">
                                Tipo Doc: <xsl:value-of select="TpoDocRef"/>
                                <script>
                                  <xsl:value-of select="TpoDocRef"/>".formDTEName()
                                </script>&#160; Folio: <xsl:value-of select='FolioRef'/> y  fecha de emisión <xsl:value-of select="FchRef"/>
                                
                              </xsl:for-each>
                            </td>
                            <td>
                            <xsl:for-each select="DTE/Documento/Encabezado/Totales/ImptoReten">
                                <table  style="width: 99%" cellspacing="0" cellpadding="0" border="0">
                                  <tr>
                                    <td style="width: 6.8cm; border-left:1px solid navy" align="left" class="cabeceraClientes">
                                      Codigo: [<xsl:value-of select="TipoImp" />] &#160;
                                      Tasa: [<xsl:value-of select="TasaImp" />]% &#160;
                                      Impuesto: [<xsl:value-of select='format-number(MontoImp, "###.##0", "clp")'/>] </td>
                                  </tr>
                                </table>
                            </xsl:for-each>
                            </td>
                        </tr>  
                      </table>

                  </td>
                </tr>
                
                
                <tr>
                  <td>
                    
                    <!-- ******** Timbre - Acuse  ******** -->
                    <xsl:choose>
                      <xsl:when test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
                        <table  style="width: 99%;" cellspacing="0" cellpadding="0" border="0">
                          <tr>
                            <td style="width: 40cm;border-right:1px solid navy" rowspan="4" valign="top">
                              <table width="99%"  border="0" cellpadding="0" cellspacing="0">
                                <xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='33'] |  DTE/Documento/Encabezado/IdDoc/TipoDTE[.='56'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='34'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='61']">
                                  <tr>
                                    <td style="width: 30cm;">
                                      <font class="cabeceraClientes">&#160;SON1:</font>&#160;<font style="font-family:arial;font-size:6pt;">
                                        <xsl:value-of select="Document/Content/DatoAdjunto[@nombre='Mescrito']"/>
                                      </font>&#160;
                                    </td>
                                  </tr>
                                </xsl:if>
                                <tr>
                                  <td style="width: 80cm; border-left:1px solid navy" class="cabeceraClientes">
                                    &#160;OBS:&#160;&#160;<font style="font-family:arial;font-size:6pt;">
                                      <xsl:value-of select="DatoAdjunto[@nombre='TextoFactura']"/>
                                    </font>&#160;
                                  </td>
                                </tr>
                                <xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
                                  <tr>
                                    <td style="width: 30cm; border-left:1px solid navy;" class="cabeceraClientes">&#160;</td>
                                    <td style="width: 30cm; " class="cabeceraClientes">&#160;</td>
                                    <td style="width: 30cm;" class="cabeceraClientes">Seguridad&#160;_____________________</td>
                                    <td style="width: 30cm;" class="cabeceraClientes">Administrador&#160;_____________________</td>
                                  </tr>
                                </xsl:if>
                                <tr>
                                  <td style="width: 30cm; border-left:1px solid navy; border-bottom:1px solid navy" class="cabeceraClientes">&#160;</td>
                                  <td style="width: 30cm; border-bottom:1px solid navy" class="cabeceraClientes">&#160;</td>
                                  <td style="width: 30cm; border-bottom:1px solid navy" class="cabeceraClientes">&#160;</td>
                                  <td style="width: 30cm; border-bottom:1px solid navy;" class="cabeceraClientes">&#160;</td>
                                </tr>
                              </table>
                            </td>
                            <xsl:if test="DTE/Documento/Encabezado/Totales/MntNeto != '' and DTE/Documento/Encabezado/IdDoc/TipoDTE[.!='52']">
                              <td style="width: 5cm;" valign="middle">
                                <font  class="cabeceraClientes">&#160;MONTO NETO :</font>
                              </td>
                              <td align="right" style="width:7cm; font-family:arial;font-size:7pt;">
                                <xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/MntNeto, "###.##0", "clp")'/>&#160;
                              </td>
                            </xsl:if>
                          </tr>
                          <xsl:if test="DTE/Documento/Encabezado/Totales/MntExe != '' and DTE/Documento/Encabezado/IdDoc/TipoDTE[.!='52']">
                            <tr>
                              <td style="width: 5cm;" valign="middle">
                                <font  class="cabeceraClientes">&#160;MONTO EXENTO :</font>
                              </td>
                              <td align="right" style="width:7cm; font-family:arial;font-size:7pt;">
                                <xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/MntExe, "###.##0", "clp")'/>&#160;
                              </td>
                            </tr>
                          </xsl:if>
                          <xsl:if test="DTE/Documento/Encabezado/Totales/IVA != '' and DTE/Documento/Encabezado/IdDoc/TipoDTE[.!='52']">
                            <tr>
                              <td style="width: 5cm;" valign="middle">
                                <font  class="cabeceraClientes">&#160;MONTO IVA 19% :</font>
                              </td>
                              <td align="right" style="width: 7cm; font-family:arial;font-size:7pt;">
                                <xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/IVA, "###.##0", "clp")'/>&#160;
                              </td>
                            </tr>
                          </xsl:if>
                          <xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='33'] |  DTE/Documento/Encabezado/IdDoc/TipoDTE[.='56'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='34'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='61']">
                            <tr>
                              <td style="width: 5cm;" valign="middle">
                                <font  class="cabeceraClientes">&#160;MONTO TOTAL :</font>
                              </td>
                              <td align="right" style="width: 7cm; font-family:arial;font-size:7pt;">
                                <xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/MntTotal, "###.##0", "clp")'/>&#160;
                              </td>
                            </tr>
                          </xsl:if>
                        </table>
                      </xsl:when>
                      <xsl:otherwise>

          <table  style="width: 99%;" cellspacing="0" cellpadding="0" border="0">
    <tr>
      
        <td style="width: 20cm;border-left:1px solid navy; border-bottom:1px solid navy" rowspan="5" valign="top">      
<table width="99%"  border="0" cellpadding="0" cellspacing="0">
	   <xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='33'] |  DTE/Documento/Encabezado/IdDoc/TipoDTE[.='56'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='34'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='61']">
          <tr>
           <td style="width: 30cm;"><font class="cabeceraClientes">&#160;SON:</font>&#160;<font style="font-family:arial;font-size:6pt;">
             <script>
               document.write(NumeroALetras(<xsl:value-of select='DTE/Documento/Encabezado/Totales/MntTotal'/>));</script> <xsl:value-of select="Document/Content/DatoAdjunto[@nombre='Mescrito']"/></font>&#160;</td>
          </tr>
		  </xsl:if>
          <tr>
           <td style="width: 30cm; " class="cabeceraClientes">&#160;OBS:&#160;&#160;<font style="font-family:arial;font-size:6pt;"><xsl:value-of select="DatoAdjunto[@nombre='TextoFactura']"/></font>&#160;</td>		   
           <td style="width: 10cm; " align="left" class="cabeceraClientes"><font style="font-family:arial;font-size:6pt;">
			<!--<xsl:for-each select="DTE/Documento/Encabezado/Totales/ImptoReten">
			Impuesto <xsl:value-of select="TasaImp" />% &#160;<xsl:value-of select='format-number(MontoImp, "###.##0", "clp")' />&#160;<br/>    
			</xsl:for-each>-->  
		   
		   </font>&#160;

     </td>
      
		 </tr>
 
        </table>		
		</td>

        <td style="width: 5cm;border-left:1px solid navy" valign="middle">
          
          <font  class="cabeceraClientes">&#160;OTROS IMPUESTO :</font>
        </td>
        <td align="right" style="width:7cm; font-family:arial;font-size:7pt; border-right:1px solid navy">
          <xsl:value-of select='format-number(sum(DTE/Documento/Encabezado/Totales/ImptoReten/MontoImp), "###.##0", "clp")'/>
        </td>
    </tr>
   <tr>
		<xsl:if test="DTE/Documento/Encabezado/Totales/MntNeto != '' and DTE/Documento/Encabezado/IdDoc/TipoDTE[.!='52']">
		 <td style="width: 5cm;border-left:1px solid navy" valign="middle"><font  class="cabeceraClientes">&#160;MONTO NETO :</font></td>		
		 <td align="right" style="width:7cm; font-family:arial;font-size:7pt; border-right:1px solid navy"><xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/MntNeto, "###.##0", "clp")'/>&#160;</td>
		</xsl:if>		
   </tr>
   <xsl:if test="DTE/Documento/Encabezado/Totales/MntExe != '' and DTE/Documento/Encabezado/IdDoc/TipoDTE[.!='52']">
      <tr>       
       <td style="width: 5cm;border-left:1px solid navy" valign="middle"><font  class="cabeceraClientes">&#160;MONTO EXENTO :</font></td>		
       <td align="right" style="width:7cm; font-family:arial;font-size:7pt; border-right:1px solid navy"><xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/MntExe, "###.##0", "clp")'/>&#160;</td>	       
      </tr>
     </xsl:if>
     <xsl:if test="DTE/Documento/Encabezado/Totales/IVA != '' and DTE/Documento/Encabezado/IdDoc/TipoDTE[.!='52']">
      <tr>       
       <td style="width: 5cm;border-left:1px solid navy" valign="middle"><font  class="cabeceraClientes">&#160;MONTO IVA 19% :</font></td>
       <td align="right" style="width: 7cm; font-family:arial;font-size:7pt; border-right:1px solid navy"><xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/IVA, "###.##0", "clp")'/>&#160;</td>	      
      </tr>
     </xsl:if>
	 <xsl:if test="DTE/Documento/Encabezado/IdDoc/TipoDTE[.='33'] |  DTE/Documento/Encabezado/IdDoc/TipoDTE[.='56'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='34'] | DTE/Documento/Encabezado/IdDoc/TipoDTE[.='61']">
	 <tr>

      <td style="width: 5cm; border-bottom:1px solid navy;border-left:1px solid navy;" valign="middle"><font  class="cabeceraClientes">&#160;MONTO TOTAL :</font></td>
      <td align="right" style="width: 7cm; font-family:arial;font-size:7pt; border-bottom:1px solid navy; border-right:1px solid navy"><xsl:value-of select='format-number(DTE/Documento/Encabezado/Totales/MntTotal, "###.##0", "clp")'/>&#160;</td>	      
    </tr>
	</xsl:if>
   </table>   
	</xsl:otherwise>
   </xsl:choose>
   
                    
<table border="0" style="width: 99%;" cellpadding="0" cellspacing="0">

     <tr style="height: 1mm;">
 
       <td width="26%"><xsl:choose>
	            <xsl:when test="Document/Content/DTE/Documento/TED/FRMT[.='']">DOCUMENTO SIN FIRMA Y TIMBRE</xsl:when>
	             <xsl:otherwise>
				 
				 
				  <div id="Timbre"><xsl:call-template name="timbre">
						<xsl:with-param name="numero-resolucion" select="'36'" />
						<xsl:with-param name="fecha-resolucion" select="'2006'" />
						<xsl:with-param name="TED" select="DTE/Documento/TED"/>
				</xsl:call-template></div>	


					 
	             </xsl:otherwise>
	            </xsl:choose></td>
       <td width="53%" valign="center">
	   <xsl:if test="Document/Content/DTE/Documento/Encabezado/IdDoc/TipoDTE[.='33'] | Document/Content/DTE/Documento/Encabezado/IdDoc/TipoDTE[.='34'] | Document/Content/DTE/Documento/Encabezado/IdDoc/TipoDTE[.='52']">
	    <table width="100%" border="0" cellspacing="0" cellpadding="0" class="tablaDatos">
					  <tr> 					   
					    <td colspan="2" align="CENTER" style="font-family:arial;font-size:6pt;">						  
						ACUSE DE RECIBO</td>
					  </tr>
					  <tr> 
					    <td colspan="2" style="font-family:arial;font-size:6pt;">
						  &#160;&#160;NOMBRE:&#160;___________________________R.U.T____________________FIRMA_________________				    
						</td>
					  </tr>
					  <tr> 
					    
					    <td style="font-family:arial;font-size:6pt; width: 5cm;">
						  &#160;&#160;FECHA:&#160;________________RECINTO___________________ 
						  
					    </td>										    
						<td style="text-align:left;font-family:arial;font-size:4.8pt;">
						       <div align="justify">EL ACUSE DE RECIBO QUE SE DECLARA EN ESTE ACTO.DE ACUERDO A LO DISPUESTO EN LA LETRA b) DEL Art.4º Y LA LETRA c) DEL Art.5º DE LA LEY 19.983.
						       ACREDITA QUE LA ENTREGA DE MERCADERIAS O SERVICIO(S) PRESTADO(S) HAN SIDO RECIBIDO(S).
							   </div>
						 
						  </td>
					  </tr>					  					  
					  
				  </table>
	   </xsl:if>
	   <table border="0" cellspacing="0" cellpadding="0">
    <tr>
    <td style="text-align:left;font-family:arial;font-size:6pt;">
	   <div align="justify">La presente factura es una respresentación gráfica del xml entregado por el proveedor de facturación electronica, a su vez creado por Puente Sur Outsourcing.
	   </div>
	</td>
	</tr>
   </table>
	  </td>
     </tr>
   </table>
   <br/><br/><br/><br/><br/>	
   
                                   <!--COLILLA CLIENTE Altolascondes.gif--> 
<xsl:if test="DatoAdjunto[@nombre='CodImpresion']= '331' or DatoAdjunto[@nombre='CodImpresion']= '561' or DatoAdjunto[@nombre='CodImpresion']= '341'">
                                      
<table width="99%" height="130" border="0" style="border-top:1px dotted black ;">
  <tr>
    <td width="50%" height="107" style="background-image: url(http://www.custodium.com/docs/cencosudc002/factura/img/Cencosudshop.gif);background-position: center;background-repeat:no-repeat;"><table width="100%"  border="0" cellpadding="0" cellspacing="0">
      <tr>
        <td height="19" colspan="2"><div align="left" class="cabeceraClientes"><xsl:value-of select="DTE/Documento/Encabezado/Emisor/RznSoc"/>&#160;</div></td>
        <td width="34%" height="19" rowspan="2" align="center" valign="bottom" class="cabeceraClientes2">Monto a Pagar</td>
      </tr>
      <tr>
        <td height="10" colspan="2"><div align="center" class="cabeceraClientes2">Cupón de Pago - Cliente</div></td>
      </tr>
      <tr>
        <td colspan="2" rowspan="2" class="cabeceraClientes2">
                	 <xsl:choose>
	 <xsl:when test="DatoAdjunto[@nombre='NumConv']=''">
	 
					<div id="CodigoBarra">
		<div id="textoBarra">
	    
	            <!-- CA4WEB CODE39 START -->
				     <xsl:call-template name="code39">
					  <xsl:with-param name="height" select="0.7" />
					
					  <xsl:with-param name="data" select="concat('*', $valor2_codigo_39, '*')" />
					  <xsl:with-param name="module-width" select="7" />
					 </xsl:call-template>
					 
					 	</div>
													</div>
				<!-- CA4WEB CODE39 FIN -->	 
				
				
    <!--script>
					       var codigo39 = '<xsl:value-of select="'000'"/><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>';
					      	</script>
						<script>
						<xsl:comment><![CDATA[
					      	document.write("<object id='code39' classid='clsid:05599D5C-30D6-49E9-8B31-6F08A23CF897' width='260' height='30' codebase='http://www.custodium.com/cabs/custodium.cab#version=1,0,0,0'>");
					      	document.write("<param name='DataToEncode' value='"+codigo39+"'/>");
					      	document.write("<param name='ModuleWidth' value='10'/>");
					      	document.write("<param name='Format' value='CODE93'/>");
					      	document.write("<param name='Mode' value='1'/>");
					      	document.write("</object>");
					        ]]>
					        </xsl:comment>
						</script-->
						
						
						
						<xsl:value-of select="'000'"/><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>&#160; 
     </xsl:when>
	 <xsl:otherwise>
	 
					<div id="CodigoBarra">
				<div id="textoBarra">
	 
	                  <!-- CA4WEB CODE39 START -->
				     <xsl:call-template name="code39">
					  <xsl:with-param name="height" select="0.7" />
					
					  <xsl:with-param name="data" select="concat('*', $valor2_codigo_39, '*')" />
					  <xsl:with-param name="module-width" select="7" />
					 </xsl:call-template>
					 
					 	</div>
													</div>
				<!-- CA4WEB CODE39 FIN -->	 
	 
	 
	   <!--script>
					       var codigo39 = '<xsl:value-of select="DatoAdjunto[@nombre='NumConv']"/><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>';
					      	</script>
						<script>
						<xsl:comment><![CDATA[
					      	document.write("<object id='code39' classid='clsid:05599D5C-30D6-49E9-8B31-6F08A23CF897' width='260' height='30' codebase='http://www.custodium.com/cabs/custodium.cab#version=1,0,0,0'>");
					      	document.write("<param name='DataToEncode' value='"+codigo39+"'/>");
					      	document.write("<param name='ModuleWidth' value='10'/>");
					      	document.write("<param name='Format' value='CODE93'/>");
					      	document.write("<param name='Mode' value='1'/>");
					      	document.write("</object>");
					        ]]>
					        </xsl:comment>
						</script-->
						
						
						<xsl:value-of select="DatoAdjunto[@nombre='NumConv']"/><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>&#160; 
	 </xsl:otherwise>
	 </xsl:choose>	 
	 
        </td>
        <td height="23" align="center"><table width="100" height="23"  border="0" cellpadding="0" cellspacing="0">
          <tr>
            <td width="128" height="21" style="width: 7cm; border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes">$</span></td>
          </tr>
        </table></td>
      </tr>
      <tr>
        <td rowspan="2" align="center"><table width="100" border="0" cellpadding="0" cellspacing="0">
          <tr>
            <td height="58" style="width: 7cm; border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><div align="center" class="cabeceraClientes2">Timbre Banco </div></td>
          </tr>
        </table>
          <div align="left"></div>
          <div align="center"></div></td>
      </tr>
      <tr>
        <td height="41" colspan="2"><table width="100%"  border="0" cellpadding="0" cellspacing="0">
		 
          <tr>
           
            <td width="30%"><span class="cabeceraClientes">Nº Convenio </span></td>
            <td width="43%"><table width="68%"  border="0" cellpadding="0" cellspacing="0">
              <tr>
                <td height="14"><div align="right" style="border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumConv']"/>&#160;</span></div></td>
              </tr>
            </table></td>
            <td width="24%">&#160;</td>
          </tr>
          <tr>
            
            <td><span class="cabeceraClientes">Nº Referencia </span></td>
            <td><table width="68%"  border="0" cellpadding="0" cellspacing="0">
              <tr>
                <td height="14"><div align="right" style="border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/>&#160;</span></div></td>
              </tr>
            </table></td>
            <td>&#160;</td>
          </tr>
          <tr>
           
            <td><span class="cabeceraClientes">Cliente</span></td>
            <td><table width="68%"  border="0" cellpadding="0" cellspacing="0">
              <tr>
                <td height="14"><div align="right" style="border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>&#160;</span></div></td>
              </tr>
            </table></td>
            <td>&#160;</td>
          </tr>
        </table></td>
        </tr>
      <tr>
        <td height="19" colspan="2"><div align="center" class="cabeceraClientes">Cancelar el dia Fechadecancelacion </div></td>
        <td height="19" align="center">&#160;</td>
      </tr>
      <tr>
        <td width="10%" height="21">&#160;</td>
        <td height="21" colspan="2"></td>
        </tr>
    </table></td>
    <td width="50%" style="border-left:1px dotted navy; background-image: url(http://www.custodium.com/docs/cencosudc002/factura/img/Cencosudshop.gif);background-position: center;background-repeat:no-repeat;"><table width="100%"  border="0">
      <tr>
      <td width="50%" height="107" style="background-image: url(http://www.custodium.com/docs/cencosudc002/factura/img/Cencosudshop.gif);background-position: center;background-repeat:no-repeat;"><table width="100%"  border="0" cellpadding="0" cellspacing="0">
      <tr>
        <td height="19" colspan="2"><div align="left" class="cabeceraClientes"><xsl:value-of select="DTE/Documento/Encabezado/Emisor/RznSoc"/>&#160;</div></td>
        <td width="34%" height="19" rowspan="2" align="center" valign="bottom" class="cabeceraClientes2">Monto a Pagar</td>
      </tr>
      <tr>
        <td height="10" colspan="2"><div align="center" class="cabeceraClientes2">Cupón de Pago - Cliente</div></td>
      </tr>
      <tr>
        <td colspan="2" rowspan="2" class="cabeceraClientes2">
        
        	 <xsl:choose>
	 <xsl:when test="DatoAdjunto[@nombre='NumConv']=''">
	 
					<div id="CodigoBarra">
		<div id="textoBarra">
	    
	            <!-- CA4WEB CODE39 START -->
				     <xsl:call-template name="code39">
					  <xsl:with-param name="height" select="0.7" />
					
					  <xsl:with-param name="data" select="concat('*', $valor2_codigo_39, '*')" />
					  <xsl:with-param name="module-width" select="7" />
					 </xsl:call-template>
					 
					 	</div>
													</div>

						
						
						<xsl:value-of select="'000'"/><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>&#160; 
     </xsl:when>
	 <xsl:otherwise>
	 
					<div id="CodigoBarra">
				<div id="textoBarra">
	 
	                  <!-- CA4WEB CODE39 START -->
				     <xsl:call-template name="code39">
					  <xsl:with-param name="height" select="0.7" />
					
					  <xsl:with-param name="data" select="concat('*', $valor2_codigo_39, '*')" />
					  <xsl:with-param name="module-width" select="7" />
					 </xsl:call-template>
					 
					 	</div>
													</div>

						
						<xsl:value-of select="DatoAdjunto[@nombre='NumConv']"/><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>&#160; 
	 </xsl:otherwise>
	 </xsl:choose>


	 
	 </td>	 
        <td height="23" align="center"><table width="100" height="23"  border="0" cellpadding="0" cellspacing="0">
          <tr>
            <td width="128" height="21" style="width: 7cm; border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes">$</span></td>
          </tr>
        </table></td>
      </tr>
      <tr>
        <td rowspan="2" align="center"><table width="100" border="0" cellpadding="0" cellspacing="0">
          <tr>
            <td height="58" style="width: 7cm; border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><div align="center" class="cabeceraClientes2">Timbre Banco </div></td>
          </tr>
        </table>
          <div align="left"></div>
          <div align="center"></div></td>
      </tr>
      <tr>
        <td height="41" colspan="2"><table width="100%"  border="0" cellpadding="0" cellspacing="0">
		
          <tr>
         
            <td width="30%"><span class="cabeceraClientes">Nº Convenio </span></td>
            <td width="43%"><table width="68%"  border="0" cellpadding="0" cellspacing="0">
              <tr>
                <td height="14"><div align="right" style="border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumConv']"/>&#160;</span></div></td>
              </tr>
            </table></td>
            <td width="24%">&#160;</td>
          </tr>
          <tr>
           
            <td><span class="cabeceraClientes">Nº Referencia </span></td>
            <td><table width="68%"  border="0" cellpadding="0" cellspacing="0">
              <tr>
                <td height="14"><div align="right" style="border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes"><xsl:value-of select="DatoAdjunto[@nombre='NumRef']"/>&#160;</span></div></td>
              </tr>
            </table></td>
            <td>&#160;</td>
          </tr>
          <tr>
            
            <td><span class="cabeceraClientes">Cliente</span></td>
            <td><table width="68%"  border="0" cellpadding="0" cellspacing="0">
              <tr>
                <td height="14"><div align="right" style="border-left:1px solid black; border-top:1px solid black; border-right:1px solid black; border-bottom:1px solid black;"><span class="cabeceraClientes"><xsl:value-of select="DTE/Documento/Encabezado/Receptor/CdgIntRecep"/>&#160;</span></div></td>
              </tr>
            </table></td>
            <td>&#160;</td>
          </tr>
        </table></td>
        </tr>
      <tr>
        <td height="19" colspan="2"><div align="center" class="cabeceraClientes">Cancelar el dia Fechadecancelacion </div></td>
        <td height="19" align="center">&#160;</td>
      </tr>
      <tr>
        <td width="10%" height="21">&#160;</td>
        <td height="21" colspan="2"></td>
        </tr>
    </table></td>
        </tr>
    </table></td>
  </tr>
</table>
</xsl:if>   

</td>
</tr>
</table>
</td>
</tr>					
</table>
</body>
</html>
</xsl:template>


<!-- ============================ -->
 <!--  SOPORTE PARA CA4WEB INICIO  -->
 <!-- ============================ -->
 
<xsl:template name="timbre">
	<xsl:param name="numero-resolucion" />
	<xsl:param name="fecha-resolucion" />
	<xsl:param name="TED" />

	<div style="height: 3.5cm;">
		<center>
		<xsl:choose>
		<xsl:when test="$TED/FRMT[.='']">DOCUMENTO SIN FIRMA Y TIMBRE</xsl:when>
		<xsl:otherwise>
		   <xsl:call-template name="pdf417">
		    <xsl:with-param name="width" select="8.4" />
		    <xsl:with-param name="height" select="2.5" />
		    <xsl:with-param name="aspect" select="2" />
		    <xsl:with-param name="columns" select="13" />
		    <xsl:with-param name="ecc" select="5" />
		    <xsl:with-param name="module-width" select="7" />
		    <xsl:with-param name="data">
		     <xsl:for-each select="$TED">&lt;TED version="1.0"&gt;&lt;DD&gt;&lt;RE&gt;<xsl:value-of select="DD/RE"/>&lt;/RE&gt;&lt;TD&gt;<xsl:value-of select="DD/TD"/>&lt;/TD&gt;&lt;F&gt;<xsl:value-of select="DD/F"/>&lt;/F&gt;&lt;FE&gt;<xsl:value-of select="DD/FE"/>&lt;/FE&gt;&lt;RR&gt;<xsl:value-of select="DD/RR"/>&lt;/RR&gt;&lt;RSR&gt;<xsl:call-template name="xml_escape"><xsl:with-param name="input"><xsl:value-of select="DD/RSR"/>
		      </xsl:with-param></xsl:call-template>&lt;/RSR>&lt;MNT&gt;<xsl:value-of select="DD/MNT"/>&lt;/MNT&gt;&lt;IT1><xsl:call-template name="xml_escape"><xsl:with-param name="input"><xsl:value-of select="DD/IT1"/></xsl:with-param></xsl:call-template>&lt;/IT1>&lt;CAF version="1.0"&gt;&lt;DA&gt;&lt;RE&gt;<xsl:value-of select="DD/CAF/DA/RE"/>&lt;/RE&gt;&lt;RS&gt;<xsl:call-template name="xml_escape"><xsl:with-param name="input"><xsl:value-of select="DD/CAF/DA/RS"/></xsl:with-param></xsl:call-template>&lt;/RS&gt;&lt;TD&gt;<xsl:value-of select="DD/CAF/DA/TD"/>&lt;/TD&gt;&lt;RNG&gt;&lt;D&gt;<xsl:value-of select="DD/CAF/DA/RNG/D"/>&lt;/D&gt;&lt;H&gt;<xsl:value-of select="DD/CAF/DA/RNG/H"/>&lt;/H&gt;&lt;/RNG&gt;&lt;FA&gt;<xsl:value-of select="DD/CAF/DA/FA"/>&lt;/FA&gt;&lt;RSAPK&gt;&lt;M&gt;<xsl:value-of select="DD/CAF/DA/RSAPK/M"/>&lt;/M&gt;&lt;E&gt;<xsl:value-of select="DD/CAF/DA/RSAPK/E"/>&lt;/E&gt;&lt;/RSAPK&gt;&lt;IDK&gt;<xsl:value-of select="DD/CAF/DA/IDK"/>&lt;/IDK&gt;&lt;/DA&gt;&lt;FRMA algoritmo="SHA1withRSA"&gt;<xsl:value-of select="DD/CAF/FRMA"/>&lt;/FRMA&gt;&lt;/CAF&gt;&lt;TSTED&gt;<xsl:value-of select="DD/TSTED"/>&lt;/TSTED&gt;&lt;/DD&gt;&lt;FRMT algoritmo="SHA1withRSA"&gt;<xsl:value-of select="FRMT"/>&lt;/FRMT&gt;&lt;/TED&gt;</xsl:for-each>
		    </xsl:with-param>
		   </xsl:call-template>
		 </xsl:otherwise>
		</xsl:choose>
		<div id="TimbreResolucion">
			<div id="textoResolucion" style="width: 50px;">&#160;</div>
			<div id="textoResolucion" style="width: 300px;"><!--&#160;&#160;&#160;Timbre Electr&#243;nico S.I.I. &#160; Res. <xsl:value-of select="$numero-resolucion" /> de <xsl:value-of select="$fecha-resolucion" /> <br/> Verifique documento <a href="http://www.sii.cl" target="_blank">www.sii.cl</a>--></div>
			<div id="textoResolucion" style="width: 50px;">&#160;</div>
			<div id="textoResolucion" style="width: 50px;">&#160;</div>
		</div>
		</center>
	</div>
</xsl:template>
 <!-- ============================ -->
 <!--  SOPORTE PARA CA4WEB FIN     -->
 <!-- ============================ -->
 
  <!-- ============================ -->
 <!--  SOPORTE PARA CA4WEB INICIO  -->
 <!-- ============================ -->

 <xsl:template name="ca4web-data-escape">
  <xsl:param name="url-data" />
  <xsl:variable name="data-length" select="string-length($url-data)" />
  <xsl:choose>
   <xsl:when test="$data-length = 1">
    <xsl:choose>
     <xsl:when test="$url-data = '&#x9;'">$09</xsl:when>
     <xsl:when test="$url-data = '&#xA;'">$0a</xsl:when>
     <xsl:when test="$url-data = '&#xD;'">$0d</xsl:when>
     <xsl:when test="$url-data = ' '">$20</xsl:when>
     <xsl:when test="$url-data = '&quot;'">$22</xsl:when>
     <xsl:when test="$url-data = '$'">$24</xsl:when>
     <xsl:when test="$url-data = '%'">$25</xsl:when>
     <xsl:when test="$url-data = '&amp;'">$26</xsl:when>
     <xsl:when test="$url-data = '+'">$2b</xsl:when>
     <xsl:when test="$url-data = '/'">$2f</xsl:when>
     <xsl:when test="$url-data = '&lt;'">$3c</xsl:when>
     <xsl:when test="$url-data = '='">$3d</xsl:when>
     <xsl:when test="$url-data = '&gt;'">$3e</xsl:when>
     <xsl:when test="$url-data = '?'">$3f</xsl:when>
     <xsl:otherwise><xsl:value-of select="$url-data" /></xsl:otherwise>
    </xsl:choose>
   </xsl:when>
   <xsl:otherwise>
    <xsl:call-template name="ca4web-data-escape">
     <xsl:with-param name="url-data" select="substring($url-data, 1, $data-length div 2)" />
    </xsl:call-template>
    <xsl:call-template name="ca4web-data-escape">
     <xsl:with-param name="url-data" select="substring($url-data, 1 + ($data-length div 2), $data-length - ($data-length div 2))" />
    </xsl:call-template>
   </xsl:otherwise>
  </xsl:choose>
 </xsl:template>

 <xsl:template name="pdf417">
  <xsl:param name="width" />
  <xsl:param name="height" />
  <xsl:param name="aspect" />
  <xsl:param name="columns" />
  <xsl:param name="ecc" />
  <xsl:param name="module-width" />
  <xsl:param name="data" />
  <xsl:choose>
   <xsl:when test="$custodium.ca4web">
    <xsl:variable name="img-width">
     <xsl:value-of select="format-number(($module-width * 2.54 * (($columns + 4) * 17 + 1)) div 1000, '0.0', 'us')" />
    </xsl:variable>
    <div style="width: {$width}cm; height: {$height}cm; padding-top: 0.5cm;"></div>
   </xsl:when>
   <xsl:otherwise>

   </xsl:otherwise>
  </xsl:choose>
 </xsl:template>

 <xsl:template name="bargraph">
  <xsl:param name="width" />
  <xsl:param name="height" />
  <xsl:param name="ySteps" />
  <xsl:param name="labels" />
  <xsl:param name="values" />
  <xsl:choose>
   <xsl:when test="$custodium.ca4web">
    
   </xsl:when>
   <xsl:otherwise>
				<object style="width: {$width}cm; height: {$height}cm" classid="clsid:74004BF8-0413-4C83-8D94-91208598E663" codebase="http://ws02.acepta.com/cabs/custodium.cab#version=1,0,0,0">
     <param name="X0" value="20"/>
     <param name="Y0" value="10"/>
     <param name="DxEnd" value="5"/>
     <param name="DyEnd" value="8"/>
     <param name="nLabel" value="{$ySteps}"/>
     <param name="StrLabels" value="{$labels}"/>
     <param name="StrValues" value="{$values}"/>
  	 </object>
   </xsl:otherwise>
  </xsl:choose>
 </xsl:template>
 
 
  <xsl:template name="xml_escape">
	<!--Funcion que permite el escapeo de caracteres especiales en el timbre-->
     	<xsl:param    name="input" />
    	<xsl:variable name="first" select="substring($input, 1, 1)" />
    	<xsl:variable name="tail"  select="substring($input, 2)"    />
    	    <xsl:choose>
    	    	<xsl:when test="$first = '&amp;' ">&amp;amp;</xsl:when>
    	    	<xsl:when test="$first = '&gt;'  ">&amp;gt;</xsl:when>
    	    	<xsl:when test="$first = '&lt;'  ">&amp;lt;</xsl:when>
    	    	<xsl:when test="$first = '&quot;'">&amp;quot;</xsl:when>
    	    	<xsl:when test='$first = "&apos;"'>&amp;apos;</xsl:when>
    	    	<xsl:otherwise><xsl:value-of select="$first" /></xsl:otherwise>
    	    </xsl:choose>
    	    <xsl:if test="$tail">
    	      <xsl:call-template name="xml_escape">
    	        <xsl:with-param name="input">
    	              <xsl:value-of select="$tail" />
							</xsl:with-param>
							</xsl:call-template>
    	    </xsl:if>
  </xsl:template>

</xsl:stylesheet>