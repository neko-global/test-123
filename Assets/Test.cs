using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base58;
using System;
using TweetNaclSharp;
using TweetNaclSharp.Core;

public class PhantomConnectResponseData
    {
        public string public_key;
        public string session;
    }

public class PhantomSignMessageResponseData
    {
        public string signature;
    }

public class PhantomSignAndSendTransactionResponseData
{
    public string signature;
}

public class PhantomSignMessagePayload{
    public string message;
    public string session;
}

public class PhantomSignAndSendTransactionPayload{
    public string transaction;
    public string session;
}

public class Test : MonoBehaviour
{
    KeyPair dAppKeypair;
    byte[] sharedKey;
    PhantomConnectResponseData connectData;

    public void LogByte(string key, byte[]? data){
        Debug.Log(key + " = " +String.Join(",",data));
    }

    public void setup (){
        string secretKey = "GFrdoCPDNEXCw1aoFcBhAhKmVjYtQKRZmCELj1neezfr"; // de test
        // dAppKeypair = TweetNaclSharp.Nacl.BoxKeyPair(); dungf luc thuc te\
        dAppKeypair = TweetNaclSharp.Nacl.BoxKeyPairFromSecretKey(Base58.Base58.Decode(secretKey));
        Debug.Log("publicKey "+ Base58.Base58.Encode(dAppKeypair.PublicKey));
        Debug.Log("secretKey "+ Base58.Base58.Encode(dAppKeypair.SecretKey));

        // setup for after connect
        connectData = new PhantomConnectResponseData(){
            public_key = "85L62nhw67fVNADqJEsgsLATpfarVLVGD1fR4KQMrnNL",
            session = "BjjkTy9aiJhkX8pGUcCMNxFPpAHSoKCDKmWKwAEeYZd2EWXGQwGjCzwKEsCj25Y7ktChKxCpoEVCSgMqNkzjrNY1Rhk8LArg6hMeVscBovSboXXEf4Y7BJJEoHcqWmEvDZ9pmhBLbDGiB5xrpTR9sBLoFwHxUjFCaSTL4v2MjeCC9ymbWsJRW2kqnhn9TB2hBepcaPWPWf7JkHK11mNsESfJWrsn"
        };
    }

    public string decryptData(string data, string nonce, byte[] sharedKey){
        LogByte("nonce",Base58.Base58.Decode(nonce));
        byte[] decryptedData  = TweetNaclSharp.Nacl.BoxOpenAfter(Base58.Base58.Decode(data), Base58.Base58.Decode(nonce), sharedKey);
         LogByte("decryptedData" ,decryptedData);
         string result = System.Text.Encoding.UTF8.GetString(decryptedData);
         return result;
        //  Debug.Log(result);
    }

    public byte[] encryptPayload(string data, string nonce, byte[] sharedKey){
        LogByte("nonce",Base58.Base58.Decode(nonce));
        
        byte[] encryptedData  = TweetNaclSharp.Nacl.BoxAfter(System.Text.Encoding.UTF8.GetBytes(data), Base58.Base58.Decode(nonce), sharedKey);

        return encryptedData;
    }

    public string generateNonce(){
        byte[] nonce = TweetNaclSharp.Nacl.RandomBytes(24);
        return Base58.Base58.Encode(nonce);
    }

    public string Base64Encode(string plainText) {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
         return System.Convert.ToBase64String(plainTextBytes);
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
        LogByte("sharedKey" ,sharedKey); //save this
        string dataResponse = decryptData(data, nonce, sharedKey);
        PhantomConnectResponseData connectData = JsonUtility.FromJson<PhantomConnectResponseData>(dataResponse);
        Debug.Log(connectData.public_key);  //save this
        Debug.Log(connectData.session); //save this
    }

    public void  signMessage(){
       string dAppPublicKey = Base58.Base58.Encode(dAppKeypair.PublicKey);
       string message = "Sign in to Nekoverse";
       byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
       Debug.Log("message = " + System.Text.Encoding.UTF8.GetString(messageBytes));
       string nonce = generateNonce();
       PhantomSignMessagePayload payload = new PhantomSignMessagePayload{
        message = Base58.Base58.Encode(messageBytes),
        session = connectData.session
       };
       string redirectLink= "https://nekoverse.net/";
       string signMessageURL = "phantom://v1/signMessage";
        Debug.Log("payload="+JsonUtility.ToJson(payload));
       string payloadStr = Base58.Base58.Encode(encryptPayload(JsonUtility.ToJson(payload), nonce, sharedKey));

       string signMessageQueryString = $"{signMessageURL}?dapp_encryption_public_key={dAppPublicKey}&nonce={nonce}&payload={payloadStr}&redirect_link={redirectLink}";
       Debug.Log(signMessageQueryString);
    }

    public void resultSignMessage(){
        // https://nekoverse.net/?nonce=PUb2suPBkf4ATMNu28XZUopxvDci8DMgy&data=co5cwpAipSQyoz5rbeuudcMAbvj1NhN65mznT8L81wnTLGEkcd2KBp3RfvmjF1AzBEX5GVoW5aaVhPVHYREZsmc8pbbmKmK8iBtpPmgzYNaYLdE1Zz6S2Zn9Cbs9GuHaNXdFe5R2an3Y6L6zj563kaGBLGzNPBVvWLm3Li1EmTuoHG3CSom6U8fwRq3n3Jccd1fQAR6jnVA8yEGpZEGXD148N61cDgfwioP38gZHCiiSusN441Tj
        string nonce = "PUb2suPBkf4ATMNu28XZUopxvDci8DMgy";
        string data = "co5cwpAipSQyoz5rbeuudcMAbvj1NhN65mznT8L81wnTLGEkcd2KBp3RfvmjF1AzBEX5GVoW5aaVhPVHYREZsmc8pbbmKmK8iBtpPmgzYNaYLdE1Zz6S2Zn9Cbs9GuHaNXdFe5R2an3Y6L6zj563kaGBLGzNPBVvWLm3Li1EmTuoHG3CSom6U8fwRq3n3Jccd1fQAR6jnVA8yEGpZEGXD148N61cDgfwioP38gZHCiiSusN441Tj";
    
        string dataResponse = decryptData(data, nonce, sharedKey);
        PhantomSignMessageResponseData signMessageData = JsonUtility.FromJson<PhantomSignMessageResponseData>(dataResponse);
        string signature =  System.Convert.ToBase64String(Base58.Base58.Decode(signMessageData.signature));
        Debug.Log("signature = " + signature);
    }

    public void signAndSendTransaction(){
       string dAppPublicKey = Base58.Base58.Encode(dAppKeypair.PublicKey);
       string nonce = generateNonce();
       byte[] transaction = new byte[]{3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,132,94,231,187,16,100,205,155,192,22,54,224,38,242,4,248,190,235,63,24,17,143,53,200,183,100,168,10,59,188,214,81,75,46,127,204,128,188,190,92,80,188,113,113,157,78,81,31,133,110,226,139,171,176,98,226,241,7,66,56,242,213,212,9,103,101,145,59,240,160,223,33,155,129,49,231,60,252,193,52,151,26,63,115,240,103,111,215,7,33,164,1,91,126,254,204,137,64,59,213,85,179,35,168,111,64,51,150,121,32,251,206,66,32,64,248,37,211,161,171,5,236,60,6,202,103,183,6,3,1,7,13,105,29,182,145,246,38,18,38,188,46,111,15,47,136,123,94,110,133,228,166,181,16,41,212,92,67,25,188,226,202,128,113,13,83,240,195,99,186,115,147,100,255,102,188,220,114,220,240,148,238,64,230,68,23,113,154,129,85,58,96,146,84,185,178,16,99,66,49,195,89,40,158,92,250,39,76,202,219,31,1,163,224,237,148,210,15,242,37,111,80,94,31,24,68,152,7,67,15,98,212,92,46,111,77,223,30,6,1,75,105,122,231,64,30,137,47,107,109,105,211,64,25,251,135,142,22,52,247,126,54,212,176,36,209,136,57,44,130,135,91,139,31,5,119,75,88,203,241,84,247,176,165,116,32,93,203,164,35,54,4,251,169,140,247,172,113,176,25,9,107,230,37,104,127,22,244,30,43,45,109,17,107,234,53,144,83,160,28,46,67,59,184,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,111,153,46,149,59,222,50,226,115,54,204,24,14,98,45,94,131,94,66,78,184,12,131,140,150,194,180,209,120,186,24,131,126,14,7,128,40,40,175,191,230,157,162,209,133,183,147,206,55,22,224,118,45,180,170,161,248,92,168,215,121,173,58,98,140,151,37,143,78,36,137,241,187,61,16,41,20,142,13,131,11,90,19,153,218,255,16,132,4,142,123,216,219,233,248,89,210,205,131,116,53,143,39,11,4,82,19,149,79,93,244,248,124,163,208,115,147,179,10,150,190,116,192,133,202,146,1,131,6,167,213,23,25,44,92,81,33,140,201,76,61,74,241,127,88,218,238,8,155,161,253,68,227,219,217,138,0,0,0,0,6,221,246,225,215,101,161,147,217,203,225,70,206,235,121,172,28,180,133,237,95,91,55,145,58,140,245,133,126,255,0,169,213,235,37,58,38,71,160,13,216,76,48,137,45,123,104,35,134,181,246,2,227,13,91,201,155,209,170,75,187,134,32,243,1,7,12,5,1,3,8,10,4,0,2,6,9,12,11,56,53,254,26,242,119,237,73,33,192,198,45,0,0,0,0,0,36,0,0,0,100,49,99,56,49,98,52,101,45,53,56,56,55,45,52,53,49,48,45,57,49,53,48,45,52,48,54,102,54,49,54,98,55,51,52,100};
      
       
       PhantomSignAndSendTransactionPayload payload = new PhantomSignAndSendTransactionPayload{
        transaction = Base58.Base58.Encode(transaction),
        session = connectData.session
       };
       string redirectLink= "https://nekoverse.net/";
       string signAndSendTransactionURL = "phantom://v1/signAndSendTransaction";
       Debug.Log("payload send and sign ="+JsonUtility.ToJson(payload));
       string payloadStr = Base58.Base58.Encode(encryptPayload(JsonUtility.ToJson(payload), nonce, sharedKey));

       string signAndSendTransactionQueryString = $"{signAndSendTransactionURL}?dapp_encryption_public_key={dAppPublicKey}&nonce={nonce}&payload={payloadStr}&redirect_link={redirectLink}";
       Debug.Log(signAndSendTransactionQueryString);
    }

    public void resultSignAndSendTransaction(){
        // https://nekoverse.net/?nonce=PUb2suPBkf4ATMNu28XZUopxvDci8DMgy&data=co5cwpAipSQyoz5rbeuudcMAbvj1NhN65mznT8L81wnTLGEkcd2KBp3RfvmjF1AzBEX5GVoW5aaVhPVHYREZsmc8pbbmKmK8iBtpPmgzYNaYLdE1Zz6S2Zn9Cbs9GuHaNXdFe5R2an3Y6L6zj563kaGBLGzNPBVvWLm3Li1EmTuoHG3CSom6U8fwRq3n3Jccd1fQAR6jnVA8yEGpZEGXD148N61cDgfwioP38gZHCiiSusN441Tj
        string nonce = "PUb2suPBkf4ATMNu28XZUopxvDci8DMgy";
        string data = "co5cwpAipSQyoz5rbeuudcMAbvj1NhN65mznT8L81wnTLGEkcd2KBp3RfvmjF1AzBEX5GVoW5aaVhPVHYREZsmc8pbbmKmK8iBtpPmgzYNaYLdE1Zz6S2Zn9Cbs9GuHaNXdFe5R2an3Y6L6zj563kaGBLGzNPBVvWLm3Li1EmTuoHG3CSom6U8fwRq3n3Jccd1fQAR6jnVA8yEGpZEGXD148N61cDgfwioP38gZHCiiSusN441Tj";
    
        string dataResponse = decryptData(data, nonce, sharedKey);
        PhantomSignAndSendTransactionResponseData signAndSendTransactionData = JsonUtility.FromJson<PhantomSignAndSendTransactionResponseData>(dataResponse);
        string signature =  signAndSendTransactionData.signature;
        Debug.Log("signature = " + signature);
    }

    public void Testing(){
        setup();
        
        // connect();
        resultConnect();

        // signMessage();
        // resultSignMessage();

        // signAndSendTransaction();
        resultSignAndSendTransaction();
        // Debug.Log(connectData.public_key);
    }
}
