package com.example.user.marina;

import android.content.Intent;
import android.support.annotation.NonNull;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.common.SignInButton;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.auth.AuthResult;
import com.google.firebase.auth.FirebaseAuth;
import com.google.firebase.auth.UserProfileChangeRequest;


public class SignUpScreen extends AppCompatActivity {

    private Button SignUpButton;
    private EditText PasswordBox;
    private EditText EmailBox;
    private EditText DNBox;
    private FirebaseAuth AuthRef;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_sign_up_screen);

        AuthRef = FirebaseAuth.getInstance();

        SignUpButton = findViewById(R.id.SignUpButton);
        PasswordBox = findViewById(R.id.PasswordBox);
        EmailBox = findViewById(R.id.EmailBox);
        DNBox = findViewById(R.id.DisplayNameBox);



        SignUpButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

                if(!PasswordBox.getText().toString().equals("") && !EmailBox.getText().toString().equals(""))
                {

                    if(AuthRef.getInstance().getCurrentUser() != null) {
                        AuthRef.getInstance().signOut();
                    }

                    AuthRef.getInstance().createUserWithEmailAndPassword(EmailBox.getText().toString(), PasswordBox.getText().toString()).addOnCompleteListener(SignUpScreen.this, new OnCompleteListener<AuthResult>() {
                        @Override
                        public void onComplete(@NonNull Task<AuthResult> task) {
                            if (task.isSuccessful())
                            {
                                UserProfileChangeRequest pr = new UserProfileChangeRequest.Builder().setDisplayName(DNBox.getText().toString()).build();
                                AuthRef.getCurrentUser().updateProfile(pr);

                                LoadMainActivity();
                            }
                            else
                            {
                                Toast.makeText(SignUpScreen.this, "Registration failed. Try another email or password", Toast.LENGTH_LONG).show();
                            }
                        }
                    });


                }

            }
        });
    }

    public void LoadMainActivity()
    {
        Intent i = new Intent(SignUpScreen.this, MainActivity.class);
        startActivity(i);
        finish();
    }

    public void LoadSignInActivity()
    {
        Intent i = new Intent(SignUpScreen.this, LogInScreen.class);
        startActivity(i);
        finish();
    }


    @Override
    public void onBackPressed() {
        LoadSignInActivity();
    }
}
