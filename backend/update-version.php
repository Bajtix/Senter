<?php
include 'root-login.php';

$table = "app" . $_POST["app"];


$date = date('Y-m-d H:i:s', strtotime(str_replace('-', '/', $_POST["date"])));



function s_escape_string($s)
{
    global $conn;
    $s = str_replace("`", "\'", $s);
    $s = str_replace("`", "\`", $s);
    $s = $conn->real_escape_string($s);
    return $s;
}


$sql = sprintf(
    "UPDATE `%s` SET `version` = '%s', `releasedate` = '%s', `changelog` = '%s', `source` = '%s', `win` = '%s', `lin` = '%s', `mac` = '%s' WHERE `%s`.`id`=%s",
    $table,
    s_escape_string($_POST["version"]),
    $date,
    s_escape_string($_POST["changelog"]),
    s_escape_string($_POST["source"]),
    s_escape_string($_POST["vwin"]),
    s_escape_string($_POST["vlin"]),
    s_escape_string($_POST["vmac"]),
    $table,
    $_POST["id"]
);

$conn->query($sql);

echo ($conn->error);


$sql = sprintf(
    "INSERT INTO `%s` (`id`, `version`, `releasedate`, `changelog`, `source`, `win`, `lin`, `mac`) VALUES ('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s') ",
    $table,
    $_POST["id"],
    s_escape_string($_POST["version"]),
    $date,
    s_escape_string($_POST["changelog"]),
    s_escape_string($_POST["source"]),
    s_escape_string($_POST["vwin"]),
    s_escape_string($_POST["vlin"]),
    s_escape_string($_POST["vmac"])
);

$conn->query($sql);
