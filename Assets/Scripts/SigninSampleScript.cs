// <copyright file="SigninSampleScript.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations

namespace SignInSample {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Google;
    using UnityEngine;
    using UnityEngine.UI;
    using Firebase;
    using Firebase.Auth;

    public class SigninSampleScript : MonoBehaviour {

        public FB fb;

        string webClientId = "159004939312-oa931hr7kkuh9k4rdl6msrr0v7hf7brl.apps.googleusercontent.com";

        private GoogleSignInConfiguration configuration;
        FirebaseAuth auth;

        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Start() {
            auth = FirebaseAuth.DefaultInstance;
            configuration = new GoogleSignInConfiguration {
                WebClientId = webClientId,
                RequestIdToken = true
            };
        }

        public void OnSignIn() {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }


        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task) {
            if (task.IsFaulted) {
                using (IEnumerator<System.Exception> enumerator =
                        task.Exception.InnerExceptions.GetEnumerator()) {
                    if (enumerator.MoveNext()) {
                        GoogleSignIn.SignInException error =
                                (GoogleSignIn.SignInException)enumerator.Current;
                        Debug.Log("Got Error: " + error.Status + " " + error.Message);
                    } else {
                        Debug.Log("Got Unexpected Exception?!?" + task.Exception);
                    }
                }
            } else if (task.IsCanceled) { 
                Debug.Log("Canceled");
            } else {
                GoogleSignInUser googleUser = task.Result;
                GetComponent<Profile>().googleUser = googleUser;
                TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
                Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask => {
                    if (authTask.IsCanceled) {
                        signInCompleted.SetCanceled();
                    } else if (authTask.IsFaulted) {
                        signInCompleted.SetException(authTask.Exception);
                    } else {
                        FirebaseUser user = authTask.Result;
                        fb.crier.ErrorMessage("Login successful!", 1);
                        fb.user = user;
                        GetComponent<Profile>().user = user;
                        GetComponent<Profile>().ProfileInit();
                        fb.CheckUser(user, googleUser);
                        signInCompleted.SetResult(authTask.Result);
                    }
                });
            }
        }
    }
}
