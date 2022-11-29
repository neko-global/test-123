using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base58;
using System;
using TweetNaclSharp;
using TweetNaclSharp.Core;

public class Test : MonoBehaviour
{
    KeyPair dAppKeypair;
    byte[] sharedKey;

    public void LogByte(string key, byte[]? data){
        Debug.Log(key + " = " +String.Join(",",data));
    }

    public void setup (){
        string secretKey = "GFrdoCPDNEXCw1aoFcBhAhKmVjYtQKRZmCELj1neezfr"; // de test
        // dAppKeypair = TweetNaclSharp.Nacl.BoxKeyPair(); dungf luc thuc te\
        dAppKeypair = TweetNaclSharp.Nacl.BoxKeyPairFromSecretKey(Base58.Base58.Decode(secretKey));
        Debug.Log("publicKey "+ Base58.Base58.Encode(dAppKeypair.PublicKey));
        Debug.Log("secretKey "+ Base58.Base58.Encode(dAppKeypair.SecretKey));
    }

    public void decryptData(string data, string nonce, byte[] sharedKey){
        LogByte("nonce",Base58.Base58.Decode(nonce));
        byte[] decryptedData  = TweetNaclSharp.Nacl.BoxAfter(Base58.Base58.Decode(data), Base58.Base58.Decode(nonce), sharedKey);
         LogByte("decryptedData" ,decryptedData);
         Debug.Log(Base58.Base58.Encode(decryptedData));
         string result = System.Text.Encoding.UTF8.GetString(decryptedData);
         Debug.Log(result);
    }

    public void connect(){
       string appURL = "https://nekoverse.net";
       string dAppPublicKey = Base58.Base58.Encode(dAppKeypair.PublicKey);
       string redirectLink= "https://nekoverse.net/";
       string cluster = "devnet";
       string connectURL = "phantom://v1/connect";
       string connectQueryString = $"{connectURL}?app_url={appURL}&dapp_encryption_public_key={dAppPublicKey}&redirect_link={redirectLink}&cluster={cluster}";
       Debug.Log(connectQueryString);
    }

    public void resultConnect(){
        //data from phantom
        string phantomEncryptionPublicKey = "B6r5zrWLJ3KqMjxRucm4Qhsaqw7j6a1xB5BCGHsx4PFb";
        string data = "KSX42sAMRbXrVM9mTCAexbyDyN376Z618qmLi39zXePH5TLVh1z6VsgVKLTyonvkVxn58XP4QL9Qynks78N47q7ccDVgY1eLNjvoDJ5c8aF1FT9Z1xBLV9mZZAaWMzbxF9rAdf6J8BwzMXpFMLy26cHfuREfwmTH3LJew6FhS9GazV32a8YKoXgtk7C5fF6Z3VyXttDodaf2tkEgc6JtiCT6AwT2sh1Fs1xkgSBza7PaGFgBJogoeH8uX39sXtnQvX35T3YSxRmF9zA9D6o3fAZjncQmeer9YLWSMnnf3tAwsDUCyrBpRtFj3WBgmkyEpYtKZziwf2eo3bL1pfz5EPJh1DKLqiwR3CBTeGoZGCtPEMkbLvdKyZMmeUpbCppVbdmkaswu9Ab2SqqAm9wRes5jXExcAJtEnahPgfj";
        string nonce = "Lp3BZLPdGgRqwM2bU8NUP8St6nr7CJ64M";
        //data from phantom

        byte[] phantomEncryptionPublicKeyByte = Base58.Base58.Decode(phantomEncryptionPublicKey);
        sharedKey = TweetNaclSharp.Nacl.BoxBefore(phantomEncryptionPublicKeyByte, dAppKeypair.SecretKey);
        LogByte("sharedKey" ,sharedKey);
        decryptData(data, nonce, sharedKey);
    }



    public void Testing(){
        setup();
        // connect();
        resultConnect();
    }
}
