using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

public class Test : MonoBehaviour {
	public GameObject LoggedInUI;
	public GameObject NotLoggedInUI;
	public GameObject Friend;
	private static string applink = "";//Put your own Applink here.

	void Awake(){
		if (!FB.IsInitialized) {
			FB.Init (InitCallBack);
		}
	}

	void InitCallBack(){
		Debug.Log ("FB has been initialized.");
		ShowUI ();
	}

	public void Login(){
		if (!FB.IsLoggedIn) {
			FB.LogInWithReadPermissions (new List<string>{ "user_friends" }, LoginCallBack);
		}
	}

	void LoginCallBack(ILoginResult result){
		if (result.Error == null) {
			Debug.Log ("FB has logged in.");
			ShowUI ();
		} else {
			Debug.Log ("Error during login: " + result.Error);
		}
	}

	void ShowUI(){
		if (FB.IsLoggedIn) {
			LoggedInUI.SetActive (true);
			NotLoggedInUI.SetActive (false);
			FB.API ("me/picture?width=100&height=100", HttpMethod.GET, PictureCallBack);
			FB.API ("me?fields=first_name", HttpMethod.GET, NameCallBack);
			FB.API ("me/friends", HttpMethod.GET, FriendCallBack);
			FB.GetAppLink (ApplinkCallback);
		} else {
			LoggedInUI.SetActive (false);
			NotLoggedInUI.SetActive (true);
		}
	}

	void PictureCallBack(IGraphResult result){
		Texture2D image = result.Texture;
		LoggedInUI.transform.FindChild ("ProfilePicture").GetComponent<Image> ().sprite = Sprite.Create (image, new Rect (0, 0, 100, 100), new Vector2 (0.5f, 0.5f));
	}

	void NameCallBack(IGraphResult result){
		IDictionary<string, object> profile = result.ResultDictionary;
		LoggedInUI.transform.FindChild ("Name").GetComponent<Text> ().text = "Hello " + profile ["first_name"];
	}

	public void LogOut(){
		FB.LogOut ();
		ShowUI ();
	}

	public void Share(){
		FB.ShareLink (new System.Uri("http://brainivore.com"), "This game is awesome!", "A description of the game.", new System.Uri("http://brainivore.com/Images/Logo.png"));
	}

	public void Invite(){
		//FB.AppRequest (message: "You should really try this game.", title: "Check this super game!");
		FB.Mobile.AppInvite(new System.Uri(applink), new System.Uri("http://brainivore.com/Images/Logo.png"), InviteCallback);
	}

	void InviteCallback(IAppInviteResult result){
		if (result.Cancelled) {
			Debug.Log ("Invite cancelled.");		
		} else if (!string.IsNullOrEmpty (result.Error)) {
			Debug.Log ("Error in invite:" + result.Error);
		} else {
			Debug.Log ("Invite was successful:" + result.RawResult);
		}
	}

	public void Challenge(){
		FB.AppRequest ("Custom message", null, new List<object>{ "app_users" }, null, null, "Data", "Challenge your friends!", ChallengeCallback);
	}

	void ChallengeCallback(IAppRequestResult result){
		if (result.Cancelled) {
			Debug.Log ("Challenge cancelled.");		
		} else if (!string.IsNullOrEmpty (result.Error)) {
			Debug.Log ("Error in challenge:" + result.Error);
		} else {
			Debug.Log ("Challenge was successful:" + result.RawResult);
		}
	}

	void FriendCallBack(IGraphResult result){
		IDictionary<string, object> data = result.ResultDictionary;
		List<object> friends = (List<object>)data ["data"];
		foreach (object obj in friends) {
			Dictionary<string, object> dictio = (Dictionary<string, object>)obj;
			CreateFriend(dictio ["name"].ToString (), dictio ["id"].ToString ());
		}
	}

	void CreateFriend(string name, string id){
		GameObject myFriend = Instantiate (Friend);
		Transform parent = LoggedInUI.transform.FindChild ("ListContainer").FindChild ("FriendList");
		myFriend.transform.SetParent (parent);
		myFriend.GetComponentInChildren<Text> ().text = name;
		FB.API(id + "/picture?width=100&height=100", HttpMethod.GET, delegate(IGraphResult result) {
			myFriend.GetComponentInChildren<Image>().sprite = Sprite.Create (result.Texture, new Rect (0, 0, 100, 100), new Vector2 (0.5f, 0.5f));
		});
	}

	void ApplinkCallback(IAppLinkResult result){
		if (string.IsNullOrEmpty (result.Error)) {
			Debug.Log ("Applink done:" + result.RawResult);
			IDictionary<string, object> dictio = result.ResultDictionary;
			if (dictio.ContainsKey ("target_url")) {
				string url = dictio ["target_url"].ToString ();
				string keyword = "request_ids=";
				int k = 0;
				while (k < url.Length - keyword.Length && !url.Substring (k, keyword.Length).Equals (keyword))
					k++;
				k += keyword.Length;
				int l = k;
				while (url [l] != '&' && url [l] != '%')
					l++;
				string id = url.Substring (k, l - k);
				FB.API ("/" + id + "_" + AccessToken.CurrentAccessToken.UserId, HttpMethod.GET, RequestCallback);
			}
		} else {
			Debug.Log ("Applink error:" + result.Error);
		}
	}

	void RequestCallback(IGraphResult result){
		Debug.Log ("Request callback");
		if (string.IsNullOrEmpty (result.Error)) {
			IDictionary<string, object> dictio = result.ResultDictionary;
			if (dictio.ContainsKey ("data"))
				Debug.Log (dictio ["data"]);
			if (dictio.ContainsKey ("id"))
				FB.API ("/" + dictio ["id"], HttpMethod.DELETE, null);
		} else {
			Debug.Log ("Error in request:" + result.Error);
		}
	}

}

