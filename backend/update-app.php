<?php
include 'root-login.php';



$sql = sprintf(
    "UPDATE `software` SET `name` = '%s', `description` = '%s', `icon` = '%s', `link` = '%s' WHERE `software`.`id` = %d ",
    $conn->real_escape_string($_POST["name"]),
    $conn->real_escape_string($_POST["description"]),
    $conn->real_escape_string($_POST["icon"]),
    $conn->real_escape_string($_POST["link"]),
    $_POST["id"]
);

$conn->query($sql);


echo ("Updated " . $_POST["id"]);
