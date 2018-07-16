/*
 * Developed by: Bar Ofner, Hadar Eliyahu
 *
 * This is the main activity file. Here the conversation with the LUIS api will be made.
 * Other than that, the request making and answer parsing will also be done.
 * This file will also include the main feature functions themselves.
 * */

package com.example.user.marina;


import android.Manifest;
import android.app.Activity;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.res.AssetManager;
import android.database.Cursor;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.ContactsContract;
import android.provider.MediaStore;
import android.support.annotation.NonNull;
import android.support.annotation.RequiresApi;
import android.support.v4.app.ActivityCompat;
import android.support.v7.app.AppCompatActivity;

import android.view.LayoutInflater;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.ScrollView;
import android.widget.TextView;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.firebase.auth.FirebaseAuth;
import com.google.firebase.database.DataSnapshot;
import com.google.firebase.database.DatabaseError;
import com.google.firebase.database.DatabaseReference;
import com.google.firebase.database.FirebaseDatabase;
import com.google.firebase.database.ValueEventListener;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.util.Calendar;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class MainActivity extends AppCompatActivity {
    // LUIS parsing
    private Map<String, Command> f = new HashMap<>();

    private Map<String, String> MediaSources = new HashMap<String, String>()
    {{
        put("google", "https://www.google.co.il/search?q=");
        put("youtube", "https://www.youtube.com/results?search_query=");
        put("amazon", "https://www.amazon.com/s/field-keywords=");
        put("spotify", "https://open.spotify.com/search/results/");
        put("wikipedia", "https://en.wikipedia.org/wiki/");
    }};


    private static final String INTENT_FIELD = "intent";
    private static final String ENTITIES_FIELD = "entities";


    // components on layout
    private ImageButton btnSendRequest;
    private EditText requestText;
    private LinearLayout messages;
    private TextView signInButton;
    private TextView signOutButton;
    private ScrollView ScrollWindow;

    //Fire Base Variables
    private FirebaseDatabase DataBaseRef;
    private FirebaseAuth AuthRef;
    public static DatabaseReference PCMessageRef;
    private DatabaseReference PhoneMessageRef;


    // volley variables
    private RequestQueue mRequestQueue;

    // LUIS details
    private final String luisAppId = "d859fc53-3e70-49ea-9f59-9fc7b9f3f8fd";
    private final String subscriptionKey = "86a2dc227c144d59b6d10a9896e4bb8b";
    private final String url = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" +
            luisAppId + "?subscription-key=" + subscriptionKey + "&q=";


    // User
    public static boolean isSigned;

    // PERMISSIONS
    private static final int PERMISSIONS_REQUEST_PHONE_CALL = 1;
    private static final int PERMISSIONS_REQUEST_READ_CONTACTS = 2;
    private static final int PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE = 3;

    public DatabaseReference getPCMessageRef()
    {
        return PCMessageRef;
    }


    @Override
    protected void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        ScrollWindow = findViewById(R.id.ScrollWindow);
        signInButton = findViewById(R.id.SignInButton);
        signOutButton = findViewById(R.id.SignOutButton);
        requestText = findViewById(R.id.RequestText);
        PCMessageRef = null;
        PhoneMessageRef = null;
        messages = findViewById(R.id.messages);
        DataBaseRef = FirebaseDatabase.getInstance();
        AuthRef = FirebaseAuth.getInstance();
        isSigned = false;


        if(AuthRef.getCurrentUser() != null)
        {
            DataBaseRef.getReference("Users/" + AuthRef.getInstance().getCurrentUser().getUid() + "/Phone/message").setValue("false");
            DataBaseRef.getReference("Users/" + AuthRef.getInstance().getCurrentUser().getUid() + "/PC/message").setValue("false");
            PushAnswer("You are connected to: " + AuthRef.getCurrentUser().getEmail());
            PCMessageRef = DataBaseRef.getReference("Users/" + AuthRef.getInstance().getCurrentUser().getUid() + "/PC/message");
            PhoneMessageRef = DataBaseRef.getReference("Users/" + AuthRef.getInstance().getCurrentUser().getUid() + "/Phone/message");

            signInButton.setVisibility(View.INVISIBLE);
            signOutButton.setVisibility(View.VISIBLE);
            isSigned = true;


            PhoneMessageRef.addValueEventListener(new ValueEventListener() {
                @Override
                public void onDataChange(DataSnapshot dataSnapshot) {
                    String value = dataSnapshot.getValue(String.class);
                    if(!value.equals("false") && !value.equals(""))
                    {
                        PhoneMessageRef.setValue("false");
                        HandleServerRequest(value);
                    }
                }

                @Override
                public void onCancelled(DatabaseError databaseError) {
                    PhoneMessageRef.setValue("false");
                    finish();
                    startActivity(getIntent());
                }
            });

        }
        else
        {
            PushAnswer("I see you haven't logged in yet. To log in please use the button above.");
        }


        //adjust buttons
        // create an event listener of make request
        btnSendRequest = findViewById(R.id.MakeRequest);
        btnSendRequest.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

                InputMethodManager inputMethodManager = (InputMethodManager) getBaseContext().getSystemService(Activity.INPUT_METHOD_SERVICE);
                inputMethodManager.hideSoftInputFromWindow(getWindow().getCurrentFocus().getWindowToken(), 0);
                String query = requestText.getText().toString();
                requestText.setText("");
                PushRequest(query);
                makeRequest(query);

            }
        });

        // pass to sign in activity

        signOutButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                AuthRef.signOut();
                PushAnswer("disconnected");
                signInButton.setVisibility(View.VISIBLE);
                signOutButton.setVisibility(View.INVISIBLE);
                isSigned = false;
            }
        });

        signInButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent i = new Intent(getBaseContext(), LogInScreen.class);
                startActivity(i);
                finish();
            }
        });



        /*
         *   Load all available functions to the map,
         *   every function has an outer name that allows to call it.
         */


        f.put("Time.What", new Command() {
        public void execute(Map<String, Entity> entityList) {
            Calendar rightNow = Calendar.getInstance();
            PushAnswer("The time now is " + String.valueOf(rightNow.get(Calendar.HOUR_OF_DAY)) + ":" + String.valueOf(rightNow.get(Calendar.MINUTE)));
        }
        });


        f.put("Search", new Command() {
            public void execute(Map<String, Entity> entityList) {
                //Sends a query to google search and opens the results in the default browser

                String RequestedMediaSource = "google";
                String RequestedMediaSourceUrl = MediaSources.get("google");


                //check if the entity list is not empty and contains the right entity type required
                if (entityList.isEmpty() || !entityList.containsKey("Query")){
                    PushAnswer("I can't understand what should I search. Please rephrase that.");
                    return;
                }

                if(entityList.containsKey("Entertainment.MediaSource"))
                {
                    if(MediaSources.containsKey(entityList.get("Entertainment.MediaSource").getEntity()))
                    {
                        RequestedMediaSource = entityList.get("Entertainment.MediaSource").getEntity();
                        RequestedMediaSourceUrl = MediaSources.get(entityList.get("Entertainment.MediaSource").getEntity());
                    }

                }

                String query = entityList.get("Query").getEntity();

                // if the user asked to that on pc
                if(entityList.containsKey("OnPC")){
                    if(isSigned)
                    {
                        PCMessageRef.setValue("search for " + query + " on " + RequestedMediaSource);
                        PushAnswer("The command was sent to your computer. If you are logged in on your computer client, the command should be executed shortly.");
                    }
                    else
                    {
                        PushAnswer("This feature can only be used by logged in users. If you wish to log in, press the log in button above.");
                    }
                }
                // local request
                else
                {
                    PushAnswer("Okay, Searching for '" + query + "' on " + RequestedMediaSource);
                    Intent viewIntent = new Intent("android.intent.action.VIEW",
                            Uri.parse(RequestedMediaSourceUrl + query));
                    startActivity(viewIntent);
                }
            }
        });

        // if the users intent is saying hello, this function will be executed
        f.put("Hello", new Command() {
            public void execute(Map<String, Entity> entityList) {
                String greets[] = new String[6];
                greets[0] = "Well hello. How can I help you";
                greets[1] = "Greetings";
                greets[2] = "Hi";
                greets[3] = "Hello";
                greets[4] = "What's up?";
                greets[5] = "I LOVE YOU";
                PushAnswer(greets[((int)Math.floor(Math.random() * 6))]);
            }
        });

        f.put("ShutDown", new Command() {
            public void execute(Map<String, Entity> entityList) {
                if(entityList.containsKey("OnPC")) {
                    if (isSigned) {
                        PCMessageRef.setValue("shutdown from phone");
                        PushAnswer("Shutdown request sent to your computer. If you are logged in on your computer client, your computer should shutdown shortly.");
                    }
                }
            }
        });

        f.put("PlayMusic", new Command() {
            @RequiresApi(api = Build.VERSION_CODES.M)
            public void execute(Map<String, Entity> entityList) {

                String full_path = "";
                String MediaResource = "";

                //check if the entity list is not empty and contains the right entity type required
                if (entityList.isEmpty() || !entityList.containsKey("Entertainment.Title"))
                {
                    PushAnswer("Sorry, I am not able to find any song name in your last request...");
                    return;
                }

                String requestedSong = entityList.get("Entertainment.Title").getEntity();

                // if the user asks to do that on pc sends the request to pc
                if(entityList.containsKey("OnPC")){
                    if(isSigned)
                    {
                        if(entityList.containsKey("Entertainment.MediaSource"))
                        {
                            PCMessageRef.setValue("Play " + requestedSong + " on " + entityList.get("Entertainment.MediaSource").getEntity());
                        }
                        else
                        {
                            PCMessageRef.setValue("Play " + requestedSong);
                        }

                        PushAnswer("Sent. song should be played shortly if it exists on your computer");
                    }
                    else
                    {
                        PushAnswer("This feature can only be used by logged in users. If you wish to log in, press the log in button above.");
                    }
                }
                else
                {
                    if(entityList.containsKey("Entertainment.MediaSource"))
                    {
                        if(MediaSources.containsKey(entityList.get("Entertainment.MediaSource").getEntity()))
                        {
                            try {
                                Intent viewIntent = new Intent("android.intent.action.VIEW",
                                        Uri.parse(MediaSources.get(entityList.get("Entertainment.MediaSource").getEntity()) + requestedSong));
                                startActivity(viewIntent);
                            }
                            catch (Exception e)
                            {
                                PushAnswer(e.toString());
                            }
                            return;
                        }

                    }

                    // find the right song path and play it
                    ContentResolver contentResolver = getContentResolver();
                    Uri songUri = MediaStore.Audio.Media.EXTERNAL_CONTENT_URI;
                    Cursor songCursor;
                    if (ActivityCompat.checkSelfPermission(getApplicationContext(), Manifest.permission.READ_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED)
                    {
                        requestPermissions(new String[]{Manifest.permission.READ_EXTERNAL_STORAGE}, PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE);
                        PushAnswer("I don't have permission for that");
                        return;
                    }

                    else
                    {
                        songCursor = contentResolver.query(songUri, null, null, null, null);
                    }


                    if(songCursor != null && songCursor.moveToFirst())
                    {
                        int songTitle = songCursor.getColumnIndex(MediaStore.Audio.Media.TITLE);

                        do {

                            if(songCursor.getString(songTitle).toLowerCase().contains(requestedSong))
                            {
                                full_path = songCursor.getString(songCursor.getColumnIndex(MediaStore.Audio.Media.DATA));
                                break;
                            }

                        } while(songCursor.moveToNext());
                        songCursor.close();
                    }

                    if(!full_path.equals(""))
                    {
                        try {
                            PushAnswer("Okay, playing: " + requestedSong);
                            Intent intent = new Intent();
                            intent.setAction(android.content.Intent.ACTION_VIEW);
                            File file = new File(full_path);
                            intent.setDataAndType(Uri.fromFile(file), "audio/*");
                            startActivity(intent);
                            return;
                        }
                        catch(Exception e){
                            PushAnswer("Encountered an error while launching the music player :(");
                            return;
                        }
                    }

                    PushAnswer("This song could not be found on this device");

                }
            }
        });

        // Open App by name
        f.put("OpenApp", new Command() {
            public void execute(Map<String, Entity> entityList) {

                String AppName = "";
                String AppPath = "";

                // check if the app name exists in the sentece
                if (entityList.isEmpty() || !entityList.containsKey("OnDevice.AppName")){
                    PushAnswer("Sorry, I am not able to find any app name in your last request...");
                    return;
                }

                String requestedApp = entityList.get("OnDevice.AppName").getEntity();

                //Opens the app requested on pc
                if(entityList.containsKey("OnPC")){
                    if(isSigned)
                    {
                        PCMessageRef.setValue("open " + requestedApp);
                        PushAnswer("Sent. app should be opened shortly if it exists on your computer");
                    }
                    else
                    {
                        PushAnswer("This feature can only be used by logged in users. If you wish to log in, press the log in button above.");
                    }


                }
                else {

                    final PackageManager pm = getPackageManager();

                    List<ApplicationInfo> packages = pm.getInstalledApplications(PackageManager.GET_META_DATA);

                    for (ApplicationInfo packageInfo : packages) {

                        if (packageInfo.loadLabel(pm).toString().toLowerCase().contains(requestedApp.toLowerCase())) {
                            AppPath = packageInfo.packageName;
                            AppName = packageInfo.loadLabel(pm).toString();
                            break; // break if app name is found
                        }
                    }

                    if (!AppName.equals("")) {
                        PushAnswer("Okay. Opening " + AppName);
                        Intent LaunchIntent = getPackageManager().getLaunchIntentForPackage(AppPath);
                        startActivity(LaunchIntent);
                        return;
                    }

                    PushAnswer("Couldn't find the app you requested");
                }
            }
        });

        // call contact by name
        f.put("Call", new Command() {
            @RequiresApi(api = Build.VERSION_CODES.M)
            public void execute(Map<String, Entity> entityList) {
                if (entityList.isEmpty() || !entityList.containsKey("Communication.ContactName")){
                    PushAnswer("Sorry, I am not able to find any contact name in your last request...");
                    return;
                }

                String contactName = entityList.get("Communication.ContactName").getEntity();
                String number="";
                String name = "";
                int indexName;
                int indexNumber;

                // requesting Phone call Permission
                if (ActivityCompat.checkSelfPermission(getApplicationContext(), Manifest.permission.CALL_PHONE) != PackageManager.PERMISSION_GRANTED)
                    requestPermissions(new String[]{Manifest.permission.CALL_PHONE}, PERMISSIONS_REQUEST_PHONE_CALL);

                // requesting Read Contact Permission
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && checkSelfPermission(Manifest.permission.READ_CONTACTS) != PackageManager.PERMISSION_GRANTED)
                    requestPermissions(new String[]{Manifest.permission.READ_CONTACTS}, PERMISSIONS_REQUEST_READ_CONTACTS);


                // requesting list of contacts
                Uri uri = ContactsContract.CommonDataKinds.Phone.CONTENT_URI;

                String[] projection = new String[] {ContactsContract.CommonDataKinds.Phone.DISPLAY_NAME,
                        ContactsContract.CommonDataKinds.Phone.NUMBER};

                Cursor contact;
                if (ActivityCompat.checkSelfPermission(MainActivity.this,
                        Manifest.permission.READ_CONTACTS) == PackageManager.PERMISSION_GRANTED) {
                    contact = getContentResolver().query(uri, projection, null, null, null);
                }
                else
                {
                    PushAnswer("I don't have the permission to do that");
                    return;
                }

                // find the match contact
                if(contact != null) {

                    indexName = contact.getColumnIndex(ContactsContract.CommonDataKinds.Phone.DISPLAY_NAME);
                    indexNumber = contact.getColumnIndex(ContactsContract.CommonDataKinds.Phone.NUMBER);
                    contact.moveToFirst();
                }
                else
                {
                    PushAnswer("Sorry, I was not able to retrieve your contact list");
                    return;
                }

                do {
                    String Name = contact.getString(indexName);
                    String Number = contact.getString(indexNumber);
                    if(Name.equalsIgnoreCase(contactName)) // contact found
                    {

                        number = Number.replace("-", "");
                        name = Name;
                        break;
                    }
                } while (contact.moveToNext());

                // close cursor
                contact.close();

                //if there is a number
                if(!number.equalsIgnoreCase(""))
                {
                    number = number.replace("-", "");
                    // check if have permission
                    if (ActivityCompat.checkSelfPermission(MainActivity.this,
                            Manifest.permission.CALL_PHONE) == PackageManager.PERMISSION_GRANTED) {
                        Intent phoneIntent = new Intent(Intent.ACTION_CALL);
                        phoneIntent.setData(Uri.parse("tel:"+ number));
                        startActivity(phoneIntent);
                        PushAnswer("Okay, Calling " + name);
                    }
                    else
                    {
                        PushAnswer("I don't have permission to do that");
                    }
                }
                else
                {
                    PushAnswer("Couldn't find the phone number of that contact");
                }
            }
        });
    }


    public void SendToFunc(String r, Map<String, Entity> entityList) {

        Command cmd;

        // find matched function to do
        cmd = f.get(r);

        if(cmd != null){
            cmd.execute(entityList);
        }
        else
        {
            PushAnswer("Sorry, I was not able to understand that.");
        }

    }

    private void makeRequest(String request) {

        btnSendRequest.setEnabled(false);
        mRequestQueue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.GET, url + request.replaceAll(" ", "+"),
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        try {
                            // split to JSON Object
                            JSONObject jObject = new JSONObject(response);

                            // get the Intent
                            String intent = jObject.getJSONObject("topScoringIntent").getString(INTENT_FIELD);

                            // get the entities
                            String typeTemp;
                            JSONArray entities = jObject.getJSONArray(ENTITIES_FIELD);
                            Map<String, Entity> entitiesList = new HashMap<>();
                            for (int i = 0; i < entities.length(); i++) {
                                typeTemp = entities.getJSONObject(i).getString("type");
                                entitiesList.put(typeTemp, new Entity(entities.getJSONObject(i).getString("entity"), typeTemp));
                            }

                            // Call the right function
                            SendToFunc(intent, entitiesList);

                        } catch (JSONException e) {
                            PushAnswer("An unknown error has occurred. Please try again.");
                        }
                        btnSendRequest.setEnabled(true);
                    }
                }, new Response.ErrorListener() {
            // on error occurred
            @Override
            public void onErrorResponse(VolleyError error) {
                PushAnswer("I having some internet issues... please try again later");
                btnSendRequest.setEnabled(true);
            }
        });
        mRequestQueue.add(stringRequest);
    }


    public void HandleServerRequest(String request) {
        int index = request.indexOf(' ');
        String KeyWord;
        if(index > -1)
            KeyWord = request.substring(0, index);
        else
        {
            makeRequest(request);
            return;
        }

        switch (KeyWord)
        {
            case "link":
                try {
                    Intent viewIntent = new Intent(Intent.ACTION_VIEW,
                            Uri.parse(request.substring(index + 1)));
                    startActivity(viewIntent);

                }
                catch (Exception e)
                {
                    PushRequest(e.toString());
                }
                break;
            case "copy":

                ClipboardManager clipboardManager = (ClipboardManager) getSystemService(Context.CLIPBOARD_SERVICE);
                ClipData clip = ClipData.newPlainText("text from pc", request.substring(index + 1));
                clipboardManager.setPrimaryClip(clip);
                break;
            default:
                PushRequest(request);
                makeRequest(request);
                break;
        }

    }

    // this function prints the answer to the gui
    public void PushAnswer(String text)
    {
        TextView v = (TextView) getLayoutInflater().inflate(R.layout.ans_text_style, null);
        v.setText(text);

        messages.addView(v);
        ScrollWindow.fullScroll(ScrollWindow.FOCUS_DOWN);

    }

    // this function prints the request to the gui
    public void PushRequest(String text)
    {
        TextView v = (TextView) getLayoutInflater().inflate(R.layout.req_text_style, null);
        v.setText(text);

        messages.addView(v);
        ScrollWindow.fullScroll(ScrollWindow.FOCUS_DOWN);
    }



    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions,
                                           @NonNull int[] grantResults) {
        switch (requestCode) {
            case PERMISSIONS_REQUEST_PHONE_CALL : {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.length > 0
                        && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                    PushAnswer("Okay, Ask me again");
                }
            }
            break;
            case PERMISSIONS_REQUEST_READ_CONTACTS:
                if (grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                    PushAnswer("Okay, Ask me again");
                }
                break;
            case PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE : {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.length > 0
                        && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                    PushAnswer("Okay, Ask me again");
                }
            }
            break;
        }
    }
}