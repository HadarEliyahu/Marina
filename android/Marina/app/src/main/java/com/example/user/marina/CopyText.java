package com.example.user.marina;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;

import com.google.firebase.auth.FirebaseAuth;
import com.google.firebase.database.DatabaseReference;
import com.google.firebase.database.FirebaseDatabase;

public class CopyText extends AppCompatActivity {

    private FirebaseDatabase DataBaseRef;
    private FirebaseAuth AuthRef;
    private DatabaseReference PCMessageRef;

    private TextView textBox;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_copy_text);

        DataBaseRef = FirebaseDatabase.getInstance();
        AuthRef = FirebaseAuth.getInstance();

        Intent i = getIntent();
        String action = i.getAction();
        String type = i.getType();

        textBox = findViewById(R.id.text1);
        textBox.setText(i.getStringExtra(Intent.EXTRA_TEXT));


        PCMessageRef = null;



        if(Intent.ACTION_SEND.equals(action) && type != null)
        {
            if(type.equals("text/plain"))
            {

                if(AuthRef.getInstance().getCurrentUser() != null)
                {
                    DataBaseRef.getReference("Users/" + AuthRef.getCurrentUser().getUid() + "/PC/message").setValue("false");
                    PCMessageRef = DataBaseRef.getReference("Users/" + AuthRef.getCurrentUser().getUid() + "/PC/message");
                    PCMessageRef.setValue("copy " + i.getStringExtra(Intent.EXTRA_TEXT));
                    Toast.makeText(CopyText.this, "Sent. Check out your PC", Toast.LENGTH_LONG).show();
                }
                else
                {
                    Toast.makeText(CopyText.this, "you are not connected", Toast.LENGTH_LONG).show();
                }
            }
            else
            {
                Toast.makeText(CopyText.this, "Not a valid text", Toast.LENGTH_LONG).show();
            }
        }
        else
        {
            Toast.makeText(CopyText.this, "Not a valid operation", Toast.LENGTH_LONG).show();
        }
        finish();
    }
}
