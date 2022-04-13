<?php


function save($dt, $nam)
{
    $json = json_encode($dt);
    echo ($json);
    file_put_contents("./.save/" . $nam, $json);
}


function read($nam)
{
    $json = file_get_contents("./.save/" . $nam);
    return json_decode($json);
}

function verify_token()
{
    $headers = getallheaders();
    $ip = $_SERVER['REMOTE_ADDR'];
    $token = $headers['token'];
    $auth = read("auth");

    if ($auth->ip != $ip || $auth->token != $token || $auth->expires < time()) {
        die("bad token");
    }
}
