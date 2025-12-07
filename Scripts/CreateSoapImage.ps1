$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force -Path "Images/Products" | Out-Null

Add-Type -AssemblyName System.Drawing

$w = 512
$h = 512
$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.Clear([System.Drawing.Color]::FromArgb(245,244,240))

$randColors = @(
    [System.Drawing.Color]::FromArgb(240,197,71),
    [System.Drawing.Color]::FromArgb(103,179,70),
    [System.Drawing.Color]::FromArgb(214,143,88),
    [System.Drawing.Color]::FromArgb(142,202,199),
    [System.Drawing.Color]::FromArgb(233,224,209)
)

$rand = New-Object System.Random
$x = 40
$y = 40
$sizes = @(120,130,110,100,90,95,105,115,125)

foreach($i in 0..6) {
    $wBox = $sizes[$rand.Next($sizes.Count)]
    $hBox = $sizes[$rand.Next($sizes.Count)]
    $c = $randColors[$rand.Next($randColors.Count)]

    $g.FillRectangle((New-Object System.Drawing.SolidBrush $c), $x, $y, $wBox, $hBox)
    $g.DrawRectangle([System.Drawing.Pens]::White, $x, $y, $wBox, $hBox)

    $x += 30
    $y += 30
    if(($x + 150) -gt $w) {
        $x = 40
        $y += 40
    }
}

$g.Dispose()
$bmp.Save("Images/Products/soap.png", [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

