eclips_e
#0001

emo — 03/24/2023 5:59 PM
ISV bornana
mci — 03/24/2023 6:44 PM
well I just glanced at Onyx and I'm gonna say
Not too impressed with it.
Lyn. — 03/25/2023 1:20 AM
would the skeld be a good shuttle?
/tmp/moony — 03/25/2023 1:26 AM
oh no
/tmp/moony — 03/25/2023 1:26 AM
very empty, undecorated, poor decor choices with the angles, basically no rooms for some reason...
mci — 03/25/2023 1:27 AM
Yeah I saw a bunch of people joke-threatening each other constantly in OOC and talking about the round
not what I'm looking for
/tmp/moony — 03/25/2023 1:29 AM
they're a closed source server which is a bit disappointing
someone could salvage their stuff
but tbf
when their code was open
it was uh
lol
russian repo management
Lyn. — 03/25/2023 1:29 AM
I mean I thought it’d look nice
if not a tad big
/tmp/moony — 03/25/2023 1:29 AM
you can in fact make skeld a good shuttle
which is why i'm concerned
Lyn. — 03/25/2023 1:29 AM
It’d be a bit big..
lots of size changes needed
mci — 03/25/2023 1:30 AM
and the last time I just left the discord one of the mods added me and asked why I left so like
ehhhh
Lyn. — 03/25/2023 1:30 AM
I’m more so concerned
/tmp/moony — 03/25/2023 1:31 AM
the mods "protect" the opinions of nazis last i looked in there so
lol
no thank you
Lyn. — 03/25/2023 1:31 AM
can I put communication terminals where emergency meeting would be
/tmp/moony — 03/25/2023 1:31 AM
comms computer is deliberately not accessible at this time
Lyn. — 03/25/2023 1:31 AM
damn
/tmp/moony — 03/25/2023 1:32 AM
(I haven't redesigned it so it can still call eshuttle among other things)
Lyn. — 03/25/2023 1:35 AM
can there be a singulo powered shuttle
mci — 03/25/2023 1:36 AM
God no
Lyn. — 03/25/2023 1:37 AM
is it a “god no” because it’s too powerful or too horrifying
tokaki — 03/25/2023 1:37 AM
both
mci — 03/25/2023 1:37 AM
Because it cannot be contained by a shuttle grid?
tokaki — 03/25/2023 1:37 AM
imagine two shuttles playing football with a singulo and containment fields
Lyn. — 03/25/2023 1:38 AM
yeah it can
I think
it worked when I tried it
mci — 03/25/2023 1:38 AM
world's deadliest game of pong.
tokaki — 03/25/2023 1:38 AM
robust pong :godo
Lyn. — 03/25/2023 1:38 AM
just give the entire crew radiation suits, a plasma glass window, and a singulo containment room
and a note that says “be REALY FUCKING CAREFUL when driving”
/tmp/moony — 03/25/2023 1:39 AM
no singuloth
Lyn. — 03/25/2023 1:39 AM
not even a singularity generator..,.,.
tokaki — 03/25/2023 1:39 AM
"baby on board"
tokaki — 03/25/2023 1:40 AM
LAROS: The emergency shuttle has been called.
(it's been 4000 years) 
Taco_llama — 03/25/2023 1:43 AM
oh okay thanks
Taco_llama — 03/25/2023 8:39 AM
is there a way to map in pressure?
/tmp/moony — Today at 12:15 AM
@rain/kyra updated tool
function Skip-Commit  {
    param (
        $toskip
    )
    $blanktree = git write-tree # We construct a new tree object.
    # With the current HEAD and the to-be-included commit as parents, adding it to the repo.
    $newhead = git commit-tree $blanktree -p HEAD -p $unmerged -m ("squash! Merge tool skipped commit {0}." -f $unmerged)
    git checkout -B (git branch --show-current) $newhead
}


Write-Output "Moony's upstream merge workflow tool."
Write-Output "This tool can be stopped at any time, i.e. to finish a merge or resolve conflicts. Simply rerun the tool after having resolved the merge with normal git cli."
Write-Output "Pay attention to any output from git! DO NOT RUN THIS ON A WORKING TREE WITH UNCOMMITTED FILES OF ANY KIND."
$target = Read-Host "Enter the branch you're syncing toward (typically upstream/master or similar)"
$refs = git log --reverse --format=format:%H HEAD.. $target

$cherryPickOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Cherry-pick","Uses git cherry pick to integrate the commit into the current branch. BE VERY CAREFUL WITH THIS."
$mergeOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Merge","Uses git merge to integrate the commit and any of it's children into the current branch."
$skipOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Skip","Skips introducing this commit."

$ackOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Acknowledge","Acknowledges you've addressed the issue."

$mergeOptions = [System.Management.Automation.Host.ChoiceDescription[]]($skipOption, $mergeOption, $cherryPickOption)
$conflictOptions = [System.Management.Automation.Host.ChoiceDescription[]]($ackOption)

$nonlinears = @()

foreach ($unmerged in $refs) {
    # Finding non-linear commits i.e. merges..
    $parents = (git log --format=format:%P -n 1 $unmerged) -split '\s+'
    if ($parents.Length -eq 1) {
        continue
    }

    # And indexing them, as we're going to need to skip them later to pick the actual merge.
    $nonlinears = $nonlinears + $parents[1..($parents.Length-1)]
}

foreach ($unmerged in $refs) {
    if ($nonlinears -contains $unmerged) {
        Write-Output ("Skipping over {0}, which we'll merge later (non-linear history encountered)." -f $unmerged)
        continue
    }

    $summary = git show --format=format:%s $unmerged

    if ($summary -ieq "automatic changelog update") {
        Write-Output ("Deliberately skipping changelog bot commit {0}." -f $unmerged)
        Write-Output "== GIT =="
        Skip-Commit $unmerged
        Write-Output "== DONE =="
        continue
    }

    $parents = (git log --format=format:%P -n 1 $unmerged) -split '\s+'

    if ($parents.Length -ne 1) {
        $mergedin = $parents[1..($parents.Length-1)]
        if (($summary -match ".*Merge tool skipping '[a-f0-9]{40}'$") -or ($summary -match ".*skipped commit [a-f0-9]{40}.*$")) {
            Write-Output ("Automatically skipping {0}, as it itself is a skip commit." -f $unmerged)
            Write-Output "== GIT =="
            Skip-Commit $unmerged
            Write-Output "== DONE =="
            continue
        } else {
            git show --format=full --summary $unmerged
            Write-Output "Which has children (note: Merging again will create a tower of merges, but fully preserves history):"

            foreach ($tomerge in $mergedin) {
                git show --format=full --summary $mergedin
            }
        }
    } else {
        git show --format=full --summary $unmerged
    }

    $response = $host.UI.PromptForChoice("Commit action?", "", $mergeOptions, -1)

    Switch ($response) {
        2 {
            Write-Output "== GIT =="
            git cherry-pick -m 1 --allow-empty $unmerged
            Write-Output "== DONE =="
        }
        1 {
            Write-Output "== GIT =="
            git merge --no-ff -m ("squash! Merge tool integrating {0}" -f $unmerged) $unmerged | Tee-Object -Variable mergeout
            Write-Output "== DONE =="
            if ($mergeout -match ".*Automatic merge failed; fix conflicts and then commit the result.*") {
                Write-Output "Please resolve the merge conflict with `git merge --continue` to resume operation."
                $host.UI.PromptForChoice("Conflicts resolved?", "", $conflictOptions, -1)
            }
        }
        0 {
            Write-Output ("Skipping {0}" -f $unmerged)
            Write-Output "== GIT =="
            Skip-Commit $unmerged
            Write-Output "== DONE =="
        }
... (3 lines left)
Collapse
upstream_merge_tool.ps1
5 KB
rain/kyra — Today at 12:15 AM
wowie
mci — Today at 12:17 AM
Updated upstream tool? 
:noted:
rain/kyra — Today at 12:17 AM
https://github.com/StationWare/stationware/pull/169
GitHub
thansks moony by Just-a-Unity-Dev · Pull Request #169 · StationWare...
very cool
thansks moony by Just-a-Unity-Dev · Pull Request #169 · StationWare...
﻿
function Skip-Commit  {
    param (
        $toskip
    )
    $blanktree = git write-tree # We construct a new tree object.
    # With the current HEAD and the to-be-included commit as parents, adding it to the repo.
    $newhead = git commit-tree $blanktree -p HEAD -p $unmerged -m ("squash! Merge tool skipped commit {0}." -f $unmerged)
    git checkout -B (git branch --show-current) $newhead
}


Write-Output "Moony's upstream merge workflow tool."
Write-Output "This tool can be stopped at any time, i.e. to finish a merge or resolve conflicts. Simply rerun the tool after having resolved the merge with normal git cli."
Write-Output "Pay attention to any output from git! DO NOT RUN THIS ON A WORKING TREE WITH UNCOMMITTED FILES OF ANY KIND."
$target = Read-Host "Enter the branch you're syncing toward (typically upstream/master or similar)"
$refs = git log --reverse --format=format:%H HEAD.. $target

$cherryPickOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Cherry-pick","Uses git cherry pick to integrate the commit into the current branch. BE VERY CAREFUL WITH THIS."
$mergeOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Merge","Uses git merge to integrate the commit and any of it's children into the current branch."
$skipOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Skip","Skips introducing this commit."

$ackOption = New-Object System.Management.Automation.Host.ChoiceDescription "&Acknowledge","Acknowledges you've addressed the issue."

$mergeOptions = [System.Management.Automation.Host.ChoiceDescription[]]($skipOption, $mergeOption, $cherryPickOption)
$conflictOptions = [System.Management.Automation.Host.ChoiceDescription[]]($ackOption)

$nonlinears = @()

foreach ($unmerged in $refs) {
    # Finding non-linear commits i.e. merges..
    $parents = (git log --format=format:%P -n 1 $unmerged) -split '\s+'
    if ($parents.Length -eq 1) {
        continue
    }

    # And indexing them, as we're going to need to skip them later to pick the actual merge.
    $nonlinears = $nonlinears + $parents[1..($parents.Length-1)]
}

foreach ($unmerged in $refs) {
    if ($nonlinears -contains $unmerged) {
        Write-Output ("Skipping over {0}, which we'll merge later (non-linear history encountered)." -f $unmerged)
        continue
    }

    $summary = git show --format=format:%s $unmerged

    if ($summary -ieq "automatic changelog update") {
        Write-Output ("Deliberately skipping changelog bot commit {0}." -f $unmerged)
        Write-Output "== GIT =="
        Skip-Commit $unmerged
        Write-Output "== DONE =="
        continue
    }

    $parents = (git log --format=format:%P -n 1 $unmerged) -split '\s+'

    if ($parents.Length -ne 1) {
        $mergedin = $parents[1..($parents.Length-1)]
        if (($summary -match ".*Merge tool skipping '[a-f0-9]{40}'$") -or ($summary -match ".*skipped commit [a-f0-9]{40}.*$")) {
            Write-Output ("Automatically skipping {0}, as it itself is a skip commit." -f $unmerged)
            Write-Output "== GIT =="
            Skip-Commit $unmerged
            Write-Output "== DONE =="
            continue
        } else {
            git show --format=full --summary $unmerged
            Write-Output "Which has children (note: Merging again will create a tower of merges, but fully preserves history):"

            foreach ($tomerge in $mergedin) {
                git show --format=full --summary $mergedin
            }
        }
    } else {
        git show --format=full --summary $unmerged
    }

    $response = $host.UI.PromptForChoice("Commit action?", "", $mergeOptions, -1)

    Switch ($response) {
        2 {
            Write-Output "== GIT =="
            git cherry-pick -m 1 --allow-empty $unmerged
            Write-Output "== DONE =="
        }
        1 {
            Write-Output "== GIT =="
            git merge --no-ff -m ("squash! Merge tool integrating {0}" -f $unmerged) $unmerged | Tee-Object -Variable mergeout
            Write-Output "== DONE =="
            if ($mergeout -match ".*Automatic merge failed; fix conflicts and then commit the result.*") {
                Write-Output "Please resolve the merge conflict with `git merge --continue` to resume operation."
                $host.UI.PromptForChoice("Conflicts resolved?", "", $conflictOptions, -1)
            }
        }
        0 {
            Write-Output ("Skipping {0}" -f $unmerged)
            Write-Output "== GIT =="
            Skip-Commit $unmerged
            Write-Output "== DONE =="
        }
    }
}
