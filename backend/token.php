<?php

$headers = getallheaders();
if ($headers["password"] != "pHKlnyXkWJHuDHf8") die("bad login");

class authtoken
{
    public $ip = "";
    public $token = "";
    public $expires = "";

    public function __construct(string $ip = "", string $token = "")
    {
        $this->ip = $ip;
        $this->token = $token;
        $this->expires = time() + 1800; // 30 mins
        //$this->expires = time() + 10; // 10 sec
    }
}

$rand_token = openssl_random_pseudo_bytes(16);
$token = bin2hex($rand_token);

$ip = $_SERVER['REMOTE_ADDR'];

$auth = new authtoken($ip, $token);


include 'util.php';
save($auth, "auth");
