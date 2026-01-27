dotnet tool install nebula-stream --global --add-source ./dist --prerelease
Skipping NuGet package signature verification.
Tools directory '/Users/minhhtamm/.dotnet/tools' is not currently on the PATH environment variable.
If you are using zsh, you can add it to your profile by running the following command:

cat << \EOF >> ~/.zprofile

# Add .NET Core SDK tools

export PATH="$PATH:/Users/minhhtamm/.dotnet/tools"
EOF

And run `zsh -l` to make it available for current session.

You can only add it to the current session by running the following command:

export PATH="$PATH:/Users/minhhtamm/.dotnet/tools"

You can invoke the tool using the following command: nebula
Tool 'nebula-stream' (version '1.0.0-preview1') was successfully installed.
