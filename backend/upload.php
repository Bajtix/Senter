<?php

include 'root-login.php';

$target_dir = "software/";

$headers = getallheaders();


$app = $headers['app'];
$version = $headers['version'];
$platform = $headers['platform'];

$r = $conn->query("SELECT * FROM `software` WHERE `id` = " . $app);

if (count($r->fetch_all()) == 0) {
    die("app invalid");
}

$r = $conn->query("SELECT * FROM `app" . $app . "` WHERE `version` = '" . $version . "'");

if (count($r->fetch_all()) == 0) {
    die("version invalid");
}

if ($platform == "") {
    die("platform invalid");
}

$target_dir = $target_dir . $app . "/";
$target_dir = $target_dir . $version . "/";
$target_file = $target_dir . $platform . ".zip";

echo ("As: " . $target_file . "\n");

$inipath = php_ini_loaded_file();

if ($inipath) {
    echo ("Loaded php.ini: " . $inipath . "\n");
} else {
    echo ("A php.ini file is not loaded\n");
}

if ($_FILES["file"]["tmp_name"] == null) {
    $max_upload = (int)(ini_get('upload_max_filesize'));
    $max_post = (int)(ini_get('post_max_size'));
    $memory_limit = (int)(ini_get('memory_limit'));
    $upload_mb = min($max_upload, $max_post, $memory_limit);
    die("max upload is " . $upload_mb);
}


if (!is_dir($target_dir)) {
    mkdir($target_dir, 0777, true);
}
echo ("Folder created: " . $target_dir . "\n");

$target_file_url = 'http://' . "mc.bajtix.xyz:8080" . "/senter/" . $target_file;

echo ("Moving file " . $target_file . " as " . $target_file_url . "\n");

if (move_uploaded_file($_FILES["file"]["tmp_name"], $target_file)) {
    echo ("Upload successful! \n");
    echo ("File is on the server as " . $target_file . "\n");
} else {
    var_dump($_FILES);
    die("Something went wrong!");
}

$modsql = sprintf(
    "UPDATE `app%s` SET `%s` = '%s' WHERE `app%s`.`version` = '%s'; ",
    $app,
    $platform,
    $target_file_url,
    $app,
    $version
);

$conn->query($modsql);

echo ($conn->error);
