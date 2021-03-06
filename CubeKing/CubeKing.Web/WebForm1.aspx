<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="Silverlight3dWeb.WebForm1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>CubeKing - v1.0 - appbyfex</title>
    <style type="text/css">
        html, body
        {
            height: 100%;
            overflow: hidden;
        }
        body
        {
            padding: 0;
            margin: 0;
        }
        #silverlightControlHost
        {
            height: 100%;
            text-align: center;
        }
    </style>
    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript">
        function GetSolve(data, w, h, d, antiscramble) {
            // Show data
            alert('This is a JavaScript alert: \r\n\r\n' +
                  'width: ' + w + ' \r\n' +
                  'height: ' + h + '\r\n' +
                  'depth: ' + d + ' \r\n\r\n' +
                  '0) up. 1) down. 2) left. 3) right. 4) front. 5) back. \r\n' +
                  data);

            // Find the control
            var control = document.getElementById("silverlightControl");

            // Solve the cube using the antiscramble
            control.Content.Page.Solve(antiscramble);
        }    
    </script>
</head>
<body>
    <div id="silverlightControlHost">
        <object data="data:application/x-silverlight-2," type="application/x-silverlight-2"
            width="100%" height="100%" id="silverlightControl">
            <param name="source" value="ClientBin/CubeKing.xap" />
            <param name="onError" value="onSilverlightError" />
            <param name="background" value="white" />
            <param name="minRuntimeVersion" value="5.0.60401.0" />
            <param name="autoUpgrade" value="true" />
            <param name="enableGPUAcceleration" value="true" />
            <param name="enableFramerateCounter" value="True" />
            <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.60401.0" style="text-decoration: none">
                <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight"
                    style="border-style: none" />
            </a>
        </object>
    </div>
</body>
</html>
