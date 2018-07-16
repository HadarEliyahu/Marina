package com.example.user.marina;

import android.content.Context;
import android.view.LayoutInflater;

import java.util.ArrayList;
import java.util.Map;

public interface Command{
    void execute(Map<String, Entity> entityList);
}