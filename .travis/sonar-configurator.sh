FILENAME=$1

# script relies on full copy of the repository, so sonar can properly detect code changes
if [ $(git rev-parse --is-shallow-repository) == true ]; then
  echo -e "\e[31mSonar needs full (not shallow) copy of the repository in order to be able to detect code files correctly" >&2
  exit 1
fi
echo -e "\e[32mFull repository copy detected"

# find correct base branch
if [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
	if [ "$TRAVIS_BRANCH" == "master" ] || [ "$TRAVIS_BRANCH" == "develop" ]; then
    # don't use base branch for long-lived branches (they should analyzed completely)
		BASE_BRANCH=""

    echo -e "\e[33mAnalyzing a long-lived branch"
	else
    # detects the likely branch current branch was branched from (so only new code is analyzed)
    BASE_BRANCH=$(git log --decorate --simplify-by-decoration --oneline \
      | grep -v "(HEAD" \
      | head -n1 \
      | sed 's/.* (\(.*\)) .*/\1/' \
      | sed 's/\(.*\), .*/\1/' \
      | sed 's/origin\///')

    echo -e "\e[33mAnalyzing short-lived (feature) branch"
  fi
else
  # base brach for PR in Travis is stored where built branch is stored otherwise
	BASE_BRANCH="$TRAVIS_BRANCH"

  echo -e "\e[33mAnalyzing pull-request"
fi

cat > "$FILENAME" << EOL
<?xml version="1.0" encoding="utf-8" ?>
<SonarQubeAnalysisProperties  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://www.sonarsource.com/msbuild/integration/2015/1">
  <!-- Connection -->
  <Property Name="sonar.host.url">https://sonarcloud.io</Property>
  <!-- Sonar project -->
  <Property Name="sonar.organization">KenticoDocs</Property>
  <Property Name="sonar.projectName">Kentico Cloud Docs – GitHub Sync</Property>
  <Property Name="sonar.projectVersion">1.$TRAVIS_JOB_NUMBER</Property>
  <Property Name="sonar.scm.provider">git</Property>
  <Property Name="sonar.scm.forceReloadAll">true</Property>
  <Property Name="sonar.links.homepage">https://github.com/KenticoDocs/cloud-docs-github-sync/</Property>
  <Property Name="sonar.links.ci">https://travis-ci.com/KenticoDocs/cloud-docs-github-sync/</Property>
  <Property Name="sonar.links.scm">https://github.com/KenticoDocs/cloud-docs-github-sync/</Property>
  <Property Name="sonar.links.issue">https://github.com/KenticoDocs/cloud-docs-github-sync/issues</Property>
  <!-- Branching -->
EOL

if [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
	cat >> "$FILENAME" << EOL
  <Property Name="sonar.branch.name">$TRAVIS_BRANCH</Property>
  <Property Name="sonar.branch.target">$BASE_BRANCH</Property>
EOL
else
	cat >> "$FILENAME" << EOL
  <Property Name="sonar.pullrequest.branch">$TRAVIS_PULL_REQUEST_BRANCH</Property>
  <Property Name="sonar.pullrequest.key">$TRAVIS_PULL_REQUEST</Property>
  <Property Name="sonar.pullrequest.base">$BASE_BRANCH</Property>
  <Property Name="sonar.pullrequest.provider">github</Property>
  <Property Name="sonar.pullrequest.github.repository">KenticoDocs/cloud-docs-github-sync.git</Property>
EOL
fi

cat >> "$FILENAME" << EOL
  <!-- C# solution -->
  <Property Name="sonar.exclusions">**/bin/**/*,**/obj/**/*</Property>
  <Property Name="sonar.cs.vstest.reportsPaths">tests_results.trx</Property>
  <Property Name="sonar.cs.opencover.reportsPaths">coverage_results.xml</Property>
</SonarQubeAnalysisProperties>
EOL

echo -e "\e[96mContent of file \e[37m\e[1m'$FILENAME':"
head -30 "$FILENAME"
