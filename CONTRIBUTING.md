# Contributing to Warden

You are more than welcome to submit any issues or pull requests to Warden repository!

The most important parts of [Warden](https://github.com/warden-stack/Warden/wiki/Warden) are [Watchers](https://github.com/warden-stack/Warden/wiki/Watcher) and [Integrations](https://github.com/warden-stack/Warden/wiki/Integration).
If you have an idea about the new one, or would like to create such feature that would be great.

Feel free to open issues about bugs or possible enhancements/extensions to this library - because of your ideas this tool is getting better!

## Submitting a Pull Request

- Make your changes in a new git branch:
```
git checkout -b my-feature master
```

- If you're creating a new [Watcher](https://github.com/warden-stack/Warden/wiki/Watcher) or [Integration](https://github.com/warden-stack/Warden/wiki/Integration) please browse the existing ones to make sure you follow the same code guidelines.
- Add your code including appropriate tests. 
- Commit your changes and add a meaningful commit message.
- Make sure your changes run locally and pass all of the tests:
```
dotnet test
```
- Push your branch to GitHub:
```
git push origin my-feature
```

Send a pull request to ```warden:master```. If there will be any suggested changes, then:
- Update your code.
- Re-run your tests.
- Commit changes to your branch (e.g. ```my-branch```).
- Push the changes to your GitHub repository that will update your pull request.

Once your pull request is merged, you can safely delete your branch and pull the changes from the main repository.