/// <binding AfterBuild='default' Clean='clean' />
/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/
import gulp from 'gulp';
import {deleteSync} from 'del';

var nodeRoot = 'node_modules/';
var targetPath = 'wwwroot/lib/';
var targetCssPath = 'wwwroot/css/';

 

gulp.task('cleanVue', function (done) {
    deleteSync(["wwwroot/dist/" + '/**/*']);
    done();
});


gulp.task('copyDist', function () {

    return gulp.src("CloudPeg.UI/dist/**/*").pipe(gulp.dest("wwwroot/dist"));
});


 